using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Login command handler.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;
	private readonly IRepositoryExtended<UserRole, Guid> _userRoleRepository;
	private readonly IRepositoryExtended<UserCompanyRole, Guid> _userCompanyRoleRepository;
	private readonly IRepositoryExtended<Role, Guid> _roleRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IJwtService _jwtService;
	private readonly IUserSessionService _sessionService;
	private readonly IDeviceInfoService _deviceInfoService;
	private readonly IUnitOfWork _unitOfWork;

	public LoginCommandHandler(
		IRepositoryExtended<User, Guid> userRepository,
		IRepositoryExtended<Company, Guid> companyRepository,
		IRepositoryExtended<UserRole, Guid> userRoleRepository,
		IRepositoryExtended<UserCompanyRole, Guid> userCompanyRoleRepository,
		IRepositoryExtended<Role, Guid> roleRepository,
		IPasswordHasher passwordHasher,
		IJwtService jwtService,
		IUserSessionService sessionService,
		IDeviceInfoService deviceInfoService,
		IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_companyRepository = companyRepository;
		_userRoleRepository = userRoleRepository;
		_userCompanyRoleRepository = userCompanyRoleRepository;
		_roleRepository = roleRepository;
		_passwordHasher = passwordHasher;
		_jwtService = jwtService;
		_sessionService = sessionService;
		_deviceInfoService = deviceInfoService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<AuthResponse>> Handle(
		LoginCommand request,
		CancellationToken cancellationToken)
	{
		// 1. Kullanıcıyı bul
		var user = await _userRepository.FirstOrDefaultAsync(
			u => u.Email == request.Email,
			cancellationToken);

		if (user == null)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.InvalidCredentials, "Invalid email or password."));
		}

		// 2. Hesap durumunu kontrol et
		if (user.Status == UserStatus.Locked)
		{
			if (user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
			{
				return Result.Failure<AuthResponse>(
					Error.Create(ErrorCodes.Auth.UserLocked, "Account is locked. Please try again later."));
			}
			else
			{
				// Kilit süresi dolmuş, kilidi kaldır
				user.Unlock();
			}
		}

		if (user.Status == UserStatus.Inactive)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.UserNotFound, "Account is inactive."));
		}

		// 3. Şifreyi doğrula
		if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
		{
			user.RecordFailedLogin();
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.InvalidCredentials, "Invalid email or password."));
		}

		// 4. Başarılı giriş
		user.RecordSuccessfulLogin();

		// 5. Device bilgilerini al
		var deviceInfo = await _deviceInfoService.GetDeviceInfoAsync(cancellationToken);

		// 6. Tenant rollerini al
		var tenantRoles = await GetUserTenantRolesAsync(user.Id, user.TenantId, cancellationToken);

		// 7. Erişilebilir şirketleri al
		var companies = await GetUserCompaniesAsync(user.Id, user.TenantId, cancellationToken);

		// 8. Session oluştur
		var sessionData = new UserSessionData
		{
			UserId = user.Id,
			TenantId = user.TenantId,
			Email = user.Email,
			FullName = user.FullName,
			TenantRoles = tenantRoles,
			CompanyRoles = new List<string>(),
			Permissions = new List<string>(),
			IpAddress = deviceInfo.IpAddress,
			UserAgent = deviceInfo.UserAgent,
			GeoLocation = deviceInfo.GeoLocation?.ToString(),
			CreatedAt = DateTime.UtcNow,
			LastActivityAt = DateTime.UtcNow,
			ExpiresAt = request.RememberMe
				? DateTime.UtcNow.AddDays(7)
				: DateTime.UtcNow.AddHours(8),
			AllowIpChange = false,
			AllowUserAgentChange = false
		};

		var sessionId = await _sessionService.CreateSessionAsync(sessionData, cancellationToken);

		// 9. JWT token oluştur
		var jwtUserData = new JwtUserData
		{
			UserId = user.Id,
			TenantId = user.TenantId,
			SessionId = sessionId,
			Email = user.Email
		};

		var accessToken = _jwtService.GenerateAccessToken(jwtUserData);
		var refreshToken = _jwtService.GenerateRefreshToken();

		// 10. Refresh token'ı user'a kaydet
		user.UpdateRefreshToken(refreshToken, _jwtService.GetRefreshTokenExpiration());
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 11. Response oluştur
		var response = new AuthResponse
		{
			UserId = user.Id,
			TenantId = user.TenantId,
			Email = user.Email,
			FullName = user.FullName,
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiration(),
			RefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiration(),
			TenantRoles = tenantRoles,
			Companies = companies
		};

		return Result.Success(response);
	}

	/// <summary>
	/// Kullanıcının tenant rollerini getirir.
	/// </summary>
	private async Task<List<string>> GetUserTenantRolesAsync(
		Guid userId,
		Guid tenantId,
		CancellationToken cancellationToken)
	{
		var userRoles = await _userRoleRepository.FindAsync(
			ur => ur.UserId == userId && ur.TenantId == tenantId && ur.IsActive,
			cancellationToken);

		var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

		var roles = await _roleRepository.FindAsync(
			r => roleIds.Contains(r.Id) && r.IsActive,
			cancellationToken);

		return roles.Select(r => r.Code).ToList();
	}

	/// <summary>
	/// Kullanıcının erişebileceği şirketleri getirir.
	/// </summary>
	private async Task<List<UserCompanyInfo>> GetUserCompaniesAsync(
		Guid userId,
		Guid tenantId,
		CancellationToken cancellationToken)
	{
		var userCompanyRoles = await _userCompanyRoleRepository.FindAsync(
			ucr => ucr.UserId == userId && ucr.TenantId == tenantId && ucr.IsActive,
			cancellationToken);

		var companyIds = userCompanyRoles.Select(ucr => ucr.CompanyId).Distinct().ToList();

		var companies = await _companyRepository.FindAsync(
			c => companyIds.Contains(c.Id) && c.Status == CompanyStatus.Active,
			cancellationToken);

		var roleIds = userCompanyRoles.Select(ucr => ucr.RoleId).Distinct().ToList();
		var roles = await _roleRepository.FindAsync(
			r => roleIds.Contains(r.Id) && r.IsActive,
			cancellationToken);

		var result = new List<UserCompanyInfo>();

		foreach (var company in companies)
		{
			var companyRoleIds = userCompanyRoles
				.Where(ucr => ucr.CompanyId == company.Id)
				.Select(ucr => ucr.RoleId)
				.ToList();

			var companyRoles = roles
				.Where(r => companyRoleIds.Contains(r.Id))
				.Select(r => r.Code)
				.ToList();

			result.Add(new UserCompanyInfo
			{
				CompanyId = company.Id,
				CompanyName = company.Name,
				CompanyCode = company.Code,
				Roles = companyRoles
			});
		}

		return result;
	}
}