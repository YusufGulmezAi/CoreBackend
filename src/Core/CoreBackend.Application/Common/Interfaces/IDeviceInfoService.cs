namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Device info servis interface.
/// HTTP Request'ten cihaz ve konum bilgilerini çýkarýr.
/// </summary>
public interface IDeviceInfoService
{
	/// <summary>
	/// Ýstemci IP adresini getirir.
	/// Proxy arkasýndaki gerçek IP'yi de tespit eder.
	/// </summary>
	string GetIpAddress();

	/// <summary>
	/// User Agent (tarayýcý/cihaz) bilgisini getirir.
	/// </summary>
	string GetUserAgent();

	/// <summary>
	/// IP adresinden coðrafi konum bilgisi getirir.
	/// </summary>
	Task<GeoLocationInfo?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);

	/// <summary>
	/// Cihaz bilgilerini toplu olarak getirir.
	/// </summary>
	Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Coðrafi konum bilgisi.
/// </summary>
public class GeoLocationInfo
{
	/// <summary>
	/// Ülke kodu (TR, US, DE vb.).
	/// </summary>
	public string? CountryCode { get; set; }

	/// <summary>
	/// Ülke adý.
	/// </summary>
	public string? Country { get; set; }

	/// <summary>
	/// Þehir.
	/// </summary>
	public string? City { get; set; }

	/// <summary>
	/// Bölge/Eyalet.
	/// </summary>
	public string? Region { get; set; }

	/// <summary>
	/// Zaman dilimi.
	/// </summary>
	public string? TimeZone { get; set; }

	/// <summary>
	/// Enlem.
	/// </summary>
	public double? Latitude { get; set; }

	/// <summary>
	/// Boylam.
	/// </summary>
	public double? Longitude { get; set; }

	public override string ToString()
	{
		return $"{City}, {Country}";
	}
}

/// <summary>
/// Cihaz bilgisi.
/// </summary>
public class DeviceInfo
{
	/// <summary>
	/// IP adresi.
	/// </summary>
	public string IpAddress { get; set; } = null!;

	/// <summary>
	/// User Agent string.
	/// </summary>
	public string UserAgent { get; set; } = null!;

	/// <summary>
	/// Coðrafi konum bilgisi.
	/// </summary>
	public GeoLocationInfo? GeoLocation { get; set; }

	/// <summary>
	/// Tarayýcý adý (parse edilmiþ).
	/// </summary>
	public string? BrowserName { get; set; }

	/// <summary>
	/// Tarayýcý versiyonu.
	/// </summary>
	public string? BrowserVersion { get; set; }

	/// <summary>
	/// Ýþletim sistemi.
	/// </summary>
	public string? OperatingSystem { get; set; }

	/// <summary>
	/// Cihaz tipi (Desktop, Mobile, Tablet).
	/// </summary>
	public string? DeviceType { get; set; }

	/// <summary>
	/// Mobil cihaz mý?
	/// </summary>
	public bool IsMobile { get; set; }
}