using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IJwtService _jwtService;
	private readonly IUserSessionService _sessionService;
	private readonly IDeviceInfoService _deviceInfoService;

	public RefreshTokenCommandHandler(
		IUnitOfWork unitOfWork,
		IJwtService jwtService,
		IUserSessionService sessionService,
		IDeviceInfoService deviceInfoService)
	{
		_unitOfWork = unitOfWork;
		_jwtService = jwtService;
		_sessionService = sessionService;
		_deviceInfoService = deviceInfoService;
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
		var user = await _unitOfWork.Users
			.FirstOrDefaultAsync(u => u.Id == userData.UserId, cancellationToken);

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

		// 8. Şirketleri al
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

	private async Task<List<string>> GetUserTenantRolesAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
	{
		var roleIds = await _unitOfWork.UserRoles
			.AsNoTracking()
			.Where(ur => ur.UserId == userId && ur.TenantId == tenantId && ur.IsActive)
			.Select(ur => ur.RoleId)
			.ToListAsync(cancellationToken);

		return await _unitOfWork.Roles
			.AsNoTracking()
			.Where(r => roleIds.Contains(r.Id) && r.IsActive)
			.Select(r => r.Code)
			.ToListAsync(cancellationToken);
	}

	private async Task<List<UserCompanyInfo>> GetUserCompaniesAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
	{
		var userCompanyRoles = await _unitOfWork.UserCompanyRoles
			.AsNoTracking()
			.Where(ucr => ucr.UserId == userId && ucr.TenantId == tenantId && ucr.IsActive)
			.ToListAsync(cancellationToken);

		if (!userCompanyRoles.Any())
			return new List<UserCompanyInfo>();

		var companyIds = userCompanyRoles.Select(ucr => ucr.CompanyId).Distinct().ToList();
		var roleIds = userCompanyRoles.Select(ucr => ucr.RoleId).Distinct().ToList();

		var companies = await _unitOfWork.Companies
			.AsNoTracking()
			.Where(c => companyIds.Contains(c.Id) && c.Status == CompanyStatus.Active)
			.ToListAsync(cancellationToken);

		var roles = await _unitOfWork.Roles
			.AsNoTracking()
			.Where(r => roleIds.Contains(r.Id) && r.IsActive)
			.ToDictionaryAsync(r => r.Id, r => r.Code, cancellationToken);

		return companies.Select(company => new UserCompanyInfo
		{
			CompanyId = company.Id,
			CompanyName = company.Name,
			CompanyCode = company.Code,
			Roles = userCompanyRoles
				.Where(ucr => ucr.CompanyId == company.Id && roles.ContainsKey(ucr.RoleId))
				.Select(ucr => roles[ucr.RoleId])
				.ToList()
		}).ToList();
	}
}