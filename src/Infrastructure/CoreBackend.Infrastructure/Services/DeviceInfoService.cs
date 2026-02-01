using Microsoft.AspNetCore.Http;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Device info servis implementasyonu.
/// HTTP Request'ten cihaz ve konum bilgilerini çıkarır.
/// </summary>
public class DeviceInfoService : IDeviceInfoService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public DeviceInfoService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	/// <summary>
	/// İstemci IP adresini getirir.
	/// </summary>
	public string GetIpAddress()
	{
		var httpContext = _httpContextAccessor.HttpContext;

		if (httpContext == null)
			return "unknown";

		// Proxy arkasındaki gerçek IP'yi al
		var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrEmpty(forwardedFor))
		{
			return forwardedFor.Split(',')[0].Trim();
		}

		var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
		if (!string.IsNullOrEmpty(realIp))
		{
			return realIp;
		}

		return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	}

	/// <summary>
	/// User Agent bilgisini getirir.
	/// </summary>
	public string GetUserAgent()
	{
		var httpContext = _httpContextAccessor.HttpContext;
		return httpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
	}

	/// <summary>
	/// IP adresinden coğrafi konum bilgisi getirir.
	/// </summary>
	public async Task<GeoLocationInfo?> GetGeoLocationAsync(
		string ipAddress,
		CancellationToken cancellationToken = default)
	{
		// Not: Gerçek uygulamada ip-api.com, ipstack.com gibi servisler kullanılır.
		// Şimdilik basit bir implementasyon.

		if (ipAddress == "127.0.0.1" || ipAddress == "::1" || ipAddress == "unknown")
		{
			return new GeoLocationInfo
			{
				Country = "Local",
				City = "Localhost",
				CountryCode = "LC"
			};
		}

		// TODO: Gerçek GeoIP servisi entegrasyonu
		return await Task.FromResult<GeoLocationInfo?>(null);
	}

	/// <summary>
	/// Cihaz bilgilerini toplu olarak getirir.
	/// </summary>
	public async Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default)
	{
		var ipAddress = GetIpAddress();
		var userAgent = GetUserAgent();
		var geoLocation = await GetGeoLocationAsync(ipAddress, cancellationToken);

		var deviceInfo = new DeviceInfo
		{
			IpAddress = ipAddress,
			UserAgent = userAgent,
			GeoLocation = geoLocation
		};

		// User Agent'ı parse et
		ParseUserAgent(userAgent, deviceInfo);

		return deviceInfo;
	}

	/// <summary>
	/// User Agent string'ini parse eder.
	/// </summary>
	private static void ParseUserAgent(string userAgent, DeviceInfo deviceInfo)
	{
		if (string.IsNullOrEmpty(userAgent) || userAgent == "unknown")
		{
			deviceInfo.BrowserName = "Unknown";
			deviceInfo.OperatingSystem = "Unknown";
			deviceInfo.DeviceType = "Unknown";
			return;
		}

		var ua = userAgent.ToLower();

		// Browser tespiti
		if (ua.Contains("edg"))
			deviceInfo.BrowserName = "Edge";
		else if (ua.Contains("chrome"))
			deviceInfo.BrowserName = "Chrome";
		else if (ua.Contains("firefox"))
			deviceInfo.BrowserName = "Firefox";
		else if (ua.Contains("safari"))
			deviceInfo.BrowserName = "Safari";
		else if (ua.Contains("opera"))
			deviceInfo.BrowserName = "Opera";
		else
			deviceInfo.BrowserName = "Other";

		// OS tespiti
		if (ua.Contains("windows"))
			deviceInfo.OperatingSystem = "Windows";
		else if (ua.Contains("mac"))
			deviceInfo.OperatingSystem = "macOS";
		else if (ua.Contains("linux"))
			deviceInfo.OperatingSystem = "Linux";
		else if (ua.Contains("android"))
			deviceInfo.OperatingSystem = "Android";
		else if (ua.Contains("iphone") || ua.Contains("ipad"))
			deviceInfo.OperatingSystem = "iOS";
		else
			deviceInfo.OperatingSystem = "Other";

		// Device type tespiti
		if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
		{
			deviceInfo.DeviceType = "Mobile";
			deviceInfo.IsMobile = true;
		}
		else if (ua.Contains("tablet") || ua.Contains("ipad"))
		{
			deviceInfo.DeviceType = "Tablet";
			deviceInfo.IsMobile = true;
		}
		else
		{
			deviceInfo.DeviceType = "Desktop";
			deviceInfo.IsMobile = false;
		}
	}
}