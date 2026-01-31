namespace CoreBackend.Application.Common.Models.Session;

/// <summary>
/// Cihaz/Browser bilgileri.
/// </summary>
public sealed record DeviceInfo
{
	/// <summary>
	/// IP adresi.
	/// </summary>
	public required string IpAddress { get; init; }

	/// <summary>
	/// User-Agent string.
	/// </summary>
	public required string UserAgent { get; init; }

	/// <summary>
	/// Browser adý (Chrome, Firefox, Safari vb.).
	/// </summary>
	public string? BrowserName { get; init; }

	/// <summary>
	/// Browser versiyonu.
	/// </summary>
	public string? BrowserVersion { get; init; }

	/// <summary>
	/// Ýþletim sistemi.
	/// </summary>
	public string? OperatingSystem { get; init; }

	/// <summary>
	/// Cihaz tipi.
	/// </summary>
	public DeviceType DeviceType { get; init; }

	/// <summary>
	/// Cihaz parmak izi (opsiyonel, client tarafýndan).
	/// </summary>
	public string? DeviceFingerprint { get; init; }

	/// <summary>
	/// GeoLocation bilgisi.
	/// </summary>
	public GeoLocation? GeoLocation { get; init; }

	/// <summary>
	/// IP adresi eþleþiyor mu?
	/// </summary>
	public bool IpMatches(DeviceInfo other) =>
		IpAddress.Equals(other.IpAddress, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Browser bilgisi eþleþiyor mu? (Name + Major Version).
	/// </summary>
	public bool BrowserMatches(DeviceInfo other) =>
		string.Equals(BrowserName, other.BrowserName, StringComparison.OrdinalIgnoreCase) &&
		string.Equals(GetMajorVersion(BrowserVersion), GetMajorVersion(other.BrowserVersion), StringComparison.OrdinalIgnoreCase);

	private static string? GetMajorVersion(string? version) =>
		version?.Split('.').FirstOrDefault();
}

/// <summary>
/// Cihaz tipi.
/// </summary>
public enum DeviceType
{
	/// <summary>
	/// Bilinmeyen cihaz.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Masaüstü bilgisayar.
	/// </summary>
	Desktop = 1,

	/// <summary>
	/// Mobil cihaz.
	/// </summary>
	Mobile = 2,

	/// <summary>
	/// Tablet.
	/// </summary>
	Tablet = 3,

	/// <summary>
	/// Bot/Crawler.
	/// </summary>
	Bot = 4
}