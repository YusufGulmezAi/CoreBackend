namespace CoreBackend.Application.Common.Settings;

/// <summary>
/// Session ayarları modeli.
/// appsettings.json'dan okunur.
/// </summary>
public class SessionSettings
{
	public const string SectionName = "SessionSettings";

	/// <summary>
	/// IP değişikliğine izin ver.
	/// </summary>
	public bool AllowIpChange { get; set; } = false;

	/// <summary>
	/// User Agent değişikliğine izin ver.
	/// </summary>
	public bool AllowUserAgentChange { get; set; } = false;

	/// <summary>
	/// Varsayılan session timeout (dakika).
	/// </summary>
	public int DefaultTimeoutMinutes { get; set; } = 480;

	/// <summary>
	/// Beni hatırla timeout (dakika).
	/// </summary>
	public int RememberMeTimeoutMinutes { get; set; } = 10080;

	/// <summary>
	/// Rol bazlı timeout süreleri.
	/// </summary>
	public Dictionary<string, int> RoleTimeouts { get; set; } = new()
	{
		{ "SystemAdmin", 120 },
		{ "TenantAdmin", 240 }
	};

	/// <summary>
	/// Maksimum timeout süresi (dakika).
	/// </summary>
	public int MaxTimeoutMinutes { get; set; } = 20160;

	/// <summary>
	/// Minimum timeout süresi (dakika).
	/// </summary>
	public int MinTimeoutMinutes { get; set; } = 15;
}