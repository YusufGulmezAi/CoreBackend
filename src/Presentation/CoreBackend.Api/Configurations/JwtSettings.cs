namespace CoreBackend.Api.Configurations;

/// <summary>
/// JWT ayarları modeli.
/// appsettings.json'dan okunur.
/// </summary>
public class JwtSettings
{
	public const string SectionName = "JwtSettings";

	/// <summary>
	/// Token imzalama anahtarı.
	/// </summary>
	public string SecretKey { get; set; } = null!;

	/// <summary>
	/// Token yayıncısı.
	/// </summary>
	public string Issuer { get; set; } = null!;

	/// <summary>
	/// Token hedef kitlesi.
	/// </summary>
	public string Audience { get; set; } = null!;

	/// <summary>
	/// Access token geçerlilik süresi (dakika).
	/// </summary>
	public int AccessTokenExpirationMinutes { get; set; } = 60;

	/// <summary>
	/// Refresh token geçerlilik süresi (gün).
	/// </summary>
	public int RefreshTokenExpirationDays { get; set; } = 7;
}