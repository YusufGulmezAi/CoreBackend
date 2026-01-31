namespace CoreBackend.Api.Configurations;

/// <summary>
/// Session ayarları modeli.
/// appsettings.json'dan okunur.
/// </summary>
public class SessionSettings
{
	public const string SectionName = "SessionSettings";

	/// <summary>
	/// IP değişikliğine izin ver.
	/// false ise token farklı IP'den kullanılamaz.
	/// </summary>
	public bool AllowIpChange { get; set; } = false;

	/// <summary>
	/// User Agent (browser) değişikliğine izin ver.
	/// false ise token farklı tarayıcıdan kullanılamaz.
	/// </summary>
	public bool AllowUserAgentChange { get; set; } = false;

	/// <summary>
	/// Varsayılan session zaman aşımı süresi (dakika).
	/// Tenant veya Rol ayarı yoksa bu kullanılır.
	/// </summary>
	public int DefaultTimeoutMinutes { get; set; } = 480;

	/// <summary>
	/// "Beni Hatırla" seçeneği için session süresi (dakika).
	/// </summary>
	public int RememberMeTimeoutMinutes { get; set; } = 10080; // 7 gün

	/// <summary>
	/// Rol bazlı session süreleri.
	/// Key: Rol kodu, Value: Süre (dakika)
	/// </summary>
	public Dictionary<string, int> RoleTimeouts { get; set; } = new()
	{
		{ "SystemAdmin", 120 },
		{ "TenantAdmin", 240 }
	};

	/// <summary>
	/// Maksimum izin verilen session süresi (dakika).
	/// Güvenlik için üst limit.
	/// </summary>
	public int MaxTimeoutMinutes { get; set; } = 20160; // 14 gün

	/// <summary>
	/// Minimum izin verilen session süresi (dakika).
	/// </summary>
	public int MinTimeoutMinutes { get; set; } = 15;
}