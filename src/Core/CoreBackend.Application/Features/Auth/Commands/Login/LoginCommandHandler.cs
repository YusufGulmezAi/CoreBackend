using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IJwtService _jwtService;
	private readonly IUserSessionService _sessionService;
	private readonly IDeviceInfoService _deviceInfoService;

	public LoginCommandHandler(
		IUnitOfWork unitOfWork,
		IPasswordHasher passwordHasher,
		IJwtService jwtService,
		IUserSessionService sessionService,
		IDeviceInfoService deviceInfoService)
	{
		_unitOfWork = unitOfWork;
		_passwordHasher = passwordHasher;
		_jwtService = jwtService;
		_sessionService = sessionService;
		_deviceInfoService = deviceInfoService;
	}

	public async Task<Result<AuthResponse>> Handle(
		LoginCommand request,
		CancellationToken cancellationToken)
	{
		// 1. Kullanıcıyı bul (Global filter'ı bypass et - tüm tenant'larda ara)
		var user = await _unitOfWork.QueryIgnoreFilters<Domain.Entities.User>()
			.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

		if (user == null)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.UserNotFound, "Invalid email or password."));
		}

		// 2. Hesap durumu kontrolü
		if (user.Status == UserStatus.Locked)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.UserLocked, "Account is locked."));
		}

		if (user.Status != UserStatus.Active)
		{
			return Result.Failure<AuthResponse>(
				Error.Create(ErrorCodes.Auth.UserInactive, "Account is not active."));
		}

		// 3. Şifre doğrulama
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

		// 10. Refresh token'ı kaydet
		user.UpdateRefreshToken(refreshToken, _jwtService.GetRefreshTokenExpiration());
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 11. Response
		return Result.Success(new AuthResponse
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
		});
	}

	private async Task<List<string>> GetUserTenantRolesAsync(
		Guid userId,
		Guid tenantId,
		CancellationToken cancellationToken)
	{
		//var roleIds = await _unitOfWork.UserRoles
		//	.AsNoTracking()
		//	.Where(ur => ur.UserId == userId && ur.TenantId == tenantId && ur.IsActive)
		//	.Select(ur => ur.RoleId)
		//	.ToListAsync(cancellationToken);

		//var roles = await _unitOfWork.Roles
		//	.AsNoTracking()
		//	.Where(r => roleIds.Contains(r.Id) && r.IsActive)
		//	.Select(r => r.Code)
		//	.ToListAsync(cancellationToken);

		var roles = await _unitOfWork.UserRoles
			.Where(ur => ur.UserId == userId && ur.TenantId == tenantId && ur.IsActive)
			.Join(_unitOfWork.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
			.Where(r => r.IsActive)
			.Select(r => r.Code)
			.ToListAsync(cancellationToken);

		return roles;
	}

	private async Task<List<UserCompanyInfo>> GetUserCompaniesAsync(
		Guid userId,
		Guid tenantId,
		CancellationToken cancellationToken)
	{
		var query = from ucr in _unitOfWork.UserCompanyRoles.AsNoTracking()
					join c in _unitOfWork.Companies on ucr.CompanyId equals c.Id
					join r in _unitOfWork.Roles on ucr.RoleId equals r.Id
					where ucr.UserId == userId
						&& ucr.TenantId == tenantId
						&& ucr.IsActive
						&& c.Status == CompanyStatus.Active
						&& r.IsActive
					select new
					{
						c.Id,
						CompanyName = c.Name,
						c.Code,
						RoleCode = r.Code
					};

		var data = await query.ToListAsync(cancellationToken);

		return data
			.GroupBy(x => new { x.Id, x.CompanyName, x.Code })
			.Select(g => new UserCompanyInfo
			{
				CompanyId = g.Key.Id,
				CompanyName = g.Key.CompanyName,
				CompanyCode = g.Key.Code,
				Roles = g.Select(x => x.RoleCode).Distinct().ToList()
			})
			.ToList();



		//var userCompanyRoles = await _unitOfWork.UserCompanyRoles
		//	.AsNoTracking()
		//	.Where(ucr => ucr.UserId == userId && ucr.TenantId == tenantId && ucr.IsActive)
		//	.ToListAsync(cancellationToken);

		//if (!userCompanyRoles.Any())
		//	return new List<UserCompanyInfo>();

		//var companyIds = userCompanyRoles.Select(ucr => ucr.CompanyId).Distinct().ToList();
		//var roleIds = userCompanyRoles.Select(ucr => ucr.RoleId).Distinct().ToList();

		//var companies = await _unitOfWork.Companies
		//	.AsNoTracking()
		//	.Where(c => companyIds.Contains(c.Id) && c.Status == CompanyStatus.Active)
		//	.ToListAsync(cancellationToken);

		//var roles = await _unitOfWork.Roles
		//	.AsNoTracking()
		//	.Where(r => roleIds.Contains(r.Id) && r.IsActive)
		//	.ToListAsync(cancellationToken);

		//var result = new List<UserCompanyInfo>();

		//foreach (var company in companies)
		//{
		//	var companyRoleIds = userCompanyRoles
		//		.Where(ucr => ucr.CompanyId == company.Id)
		//		.Select(ucr => ucr.RoleId)
		//		.ToList();

		//	var companyRoles = roles
		//		.Where(r => companyRoleIds.Contains(r.Id))
		//		.Select(r => r.Code)
		//		.ToList();

		//	result.Add(new UserCompanyInfo
		//	{
		//		CompanyId = company.Id,
		//		CompanyName = company.Name,
		//		CompanyCode = company.Code,
		//		Roles = companyRoles
		//	});
		//}

		//return result;
	}
}