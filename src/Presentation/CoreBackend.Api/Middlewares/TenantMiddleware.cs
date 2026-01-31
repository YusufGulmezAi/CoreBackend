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

	// Tenant gerektirmeyen endpoint'ler
	private static readonly string[] ExcludedPaths = new[]
	{
		"/health",
		"/swagger",
		"/api/auth/login",
		"/api/auth/register",
		"/api/auth/refresh-token"
	};

	public TenantMiddleware(
		RequestDelegate next,
		ILogger<TenantMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
	{
		var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

		// Excluded path kontrolü
		if (ExcludedPaths.Any(p => path.StartsWith(p)))
		{
			await _next(context);
			return;
		}

		// Tenant Id'yi çözümle
		var tenantId = ResolveTenantId(context);

		if (tenantId.HasValue)
		{
			tenantService.SetTenantId(tenantId.Value);
			_logger.LogDebug("Tenant resolved: {TenantId}", tenantId.Value);
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