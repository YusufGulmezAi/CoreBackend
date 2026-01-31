using CoreBackend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Current user servis implementasyonu.
/// JWT Token'dan kullanıcı bilgilerini çıkarır.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	/// <summary>
	/// Mevcut kullanıcı Id (Token'dan).
	/// </summary>
	public Guid? UserId
	{
		get
		{
			var userId = GetClaimValue(ClaimTypes.NameIdentifier);
			return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
		}
	}

	/// <summary>
	/// Mevcut Tenant Id (Token'dan).
	/// </summary>
	public Guid? TenantId
	{
		get
		{
			var tenantId = GetClaimValue("tenant_id");
			return string.IsNullOrEmpty(tenantId) ? null : Guid.Parse(tenantId);
		}
	}

	/// <summary>
	/// Mevcut Session Id (Token'dan, cache key olarak kullanılır).
	/// </summary>
	public string? SessionId
	{
		get
		{
			return GetClaimValue("session_id");
		}
	}

	/// <summary>
	/// Kullanıcı oturum açmış mı?
	/// </summary>
	public bool IsAuthenticated
	{
		get
		{
			return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
		}
	}

	/// <summary>
	/// Claim değerini getirir.
	/// </summary>
	private string? GetClaimValue(string claimType)
	{
		return _httpContextAccessor.HttpContext?.User?.Claims
			.FirstOrDefault(c => c.Type == claimType)?.Value;
	}
}