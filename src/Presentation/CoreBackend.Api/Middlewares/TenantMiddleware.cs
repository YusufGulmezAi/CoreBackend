using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Api.Middlewares;

/// <summary>
/// Tenant middleware.
/// Her request'te tenant bilgisini çözümler ve TenantService'e set eder.
/// </summary>
public class TenantMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<TenantMiddleware> _logger;

	//// Tenant gerektirmeyen endpoint'ler
	//private static readonly string[] ExcludedPaths = new[]
	//{
	//	"/health",
	//	"/swagger",
	//	"/api/auth/login",
	//	"/api/auth/register",
	//	"/api/auth/refresh-token"
	//};

	public TenantMiddleware(
		RequestDelegate next,
		ILogger<TenantMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(
		HttpContext context,
		ITenantService tenantService,
		IUserSessionService sessionService,
		IDeviceInfoService deviceInfoService)
	{
		// Public endpoint'ler için skip
		var endpoint = context.GetEndpoint();
		var allowAnonymous = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();

		if (allowAnonymous != null)
		{
			await _next(context);
			return;
		}

		// Authenticated kullanıcı için tenant ve session kontrolü
		if (context.User.Identity?.IsAuthenticated == true)
		{
			var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
			var sessionIdClaim = context.User.FindFirst("session_id")?.Value;

			if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
			{
				tenantService.SetTenantId(tenantId);
			}

			// Session doğrulaması
			if (!string.IsNullOrEmpty(sessionIdClaim))
			{
				var ipAddress = deviceInfoService.GetIpAddress();
				var userAgent = deviceInfoService.GetUserAgent();

				var isValid = await sessionService.ValidateSessionAsync(
					sessionIdClaim,
					ipAddress,
					userAgent,
					context.RequestAborted);

				if (!isValid)
				{
					_logger.LogWarning("Invalid session: {SessionId}", sessionIdClaim);
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsJsonAsync(new
					{
						success = false,
						message = "Session expired or invalid.",
						errorCode = "SESSION_INVALID"
					});
					return;
				}

				// Session aktivitesini güncelle
				await sessionService.RefreshSessionActivityAsync(sessionIdClaim, context.RequestAborted);
			}
		}
		else
		{
			// Header'dan tenant alma (public API'ler için)
			var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

			if (!string.IsNullOrEmpty(tenantHeader) && Guid.TryParse(tenantHeader, out var headerTenantId))
			{
				tenantService.SetTenantId(headerTenantId);
			}
		}

		await _next(context);
	}

	/// <summary>
	/// Tenant Id'yi çözümler.
	/// Öncelik: 1. Header 2. Token Claim 3. Subdomain
	/// </summary>
	private Guid? ResolveTenantId(HttpContext context)
	{
		// 1. Header'dan al (X-Tenant-Id)
		if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
		{
			if (Guid.TryParse(headerTenantId.FirstOrDefault(), out var tenantGuid))
			{
				return tenantGuid;
			}
		}

		// 2. Token claim'den al
		var claimTenantId = context.User?.Claims
			.FirstOrDefault(c => c.Type == "tenant_id")?.Value;

		if (!string.IsNullOrEmpty(claimTenantId) && Guid.TryParse(claimTenantId, out var claimGuid))
		{
			return claimGuid;
		}

		// 3. Subdomain'den al (opsiyonel)
		// Örnek: tenant1.corebackend.com
		var host = context.Request.Host.Host;
		if (!string.IsNullOrEmpty(host) && host.Contains('.'))
		{
			var subdomain = host.Split('.')[0];
			// Burada subdomain'den tenant Id çözümlemesi yapılabilir
			// Genellikle cache veya database lookup gerektirir
			// Şimdilik sadece header ve token destekliyoruz
		}

		return null;
	}
}