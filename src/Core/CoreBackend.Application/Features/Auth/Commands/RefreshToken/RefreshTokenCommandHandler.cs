using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Refresh token command handler.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IRepositoryExtended<Company, Guid> _companyRepository;
	private readonly IRepositoryExtended<UserRole, Guid> _userRoleRepository;
	private readonly IRepositoryExtended<UserCompanyRole, Guid> _userCompanyRoleRepository;
	private readonly IRepositoryExtended<Role, Guid> _roleRepository;
	private readonly IJwtService _jwtService;
	private readonly IUserSessionService _sessionService;
	private readonly IDeviceInfoService _deviceInfoService;
	private readonly IUnitOfWork _unitOfWork;

	public RefreshTokenCommandHandler(
		IRepositoryExtended<User, Guid> userRepository,
		IRepositoryExtended<Company, Guid> companyRepository,
		IRepositoryExtended<UserRole, Guid> userRoleRepository,
		IRepositoryExtended<UserCompanyRole, Guid> userCompanyRoleRepository,
		IRepositoryExtended<Role, Guid> roleRepository,
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
		_jwtService = jwtService;
		_sessionService = sessionService;
		_deviceInfoService = deviceInfoService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<AuthResponse>> Handle(
		RefreshTokenCommand request,
		CancellationToken cancellationToken)
	{
		// 1. Expired token'dan user data çıkar
		var userData = _jwtService.GetUserDataFromExpiredToken(request.AccessToken);

		if (userData == null)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.TokenInvalid, "Invalid access token."));
		}

		// 2. Kullanıcıyı bul
		var user = await _userRepository.GetByIdAsync(userData.UserId, cancellationToken);

		if (user == null)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.UserNotFound, "User not found."));
		}

		// 3. Refresh token doğrula
		if (user.RefreshToken != request.RefreshToken)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.TokenInvalid, "Invalid refresh token."));
		}

		if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.TokenExpired, "Refresh token has expired."));
		}

		// 4. Kullanıcı durumunu kontrol et
		if (user.Status != UserStatus.Active)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.UserLocked, "User account is not active."));
		}

		// 5. Eski session'ı sil
		await _sessionService.RevokeSessionAsync(userData.SessionId, cancellationToken);

		// 6. Device bilgilerini al
		var deviceInfo = await _deviceInfoService.GetDeviceInfoAsync(cancellationToken);

		// 7. Tenant rollerini al
		var tenantRoles = await GetUserTenantRolesAsync(user.Id, user.TenantId, cancellationToken);

		// 8. Erişilebilir şirketleri al
		var companies = await GetUserCompaniesAsync(user.Id, user.TenantId, cancellationToken);

		// 9. Yeni session oluştur
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
			ExpiresAt = DateTime.UtcNow.AddHours(8),
			AllowIpChange = false,
			AllowUserAgentChange = false
		};

		var sessionId = await _sessionService.CreateSessionAsync(sessionData, cancellationToken);

		// 10. Yeni JWT token oluştur
		var jwtUserData = new JwtUserData
		{
			UserId = user.Id,
			TenantId = user.TenantId,
			SessionId = sessionId,
			Email = user.Email
		};

		var accessToken = _jwtService.GenerateAccessToken(jwtUserData);
		var refreshToken = _jwtService.GenerateRefreshToken();

		// 11. Yeni refresh token'ı kaydet
		user.UpdateRefreshToken(refreshToken, _jwtService.GetRefreshTokenExpiration());
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 12. Response oluştur
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