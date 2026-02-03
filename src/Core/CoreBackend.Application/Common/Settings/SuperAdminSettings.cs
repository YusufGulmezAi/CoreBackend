namespace CoreBackend.Application.Common.Settings;

/// <summary>
/// Super Admin başlangıç ayarları.
/// Environment variables veya User Secrets'tan okunur.
/// </summary>
public class SuperAdminSettings
{
	public const string SectionName = "SuperAdmin";

	/// <summary>
	/// Super Admin email adresi.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Super Admin başlangıç şifresi.
	/// İlk girişte değiştirilmesi zorunlu olmalı.
	/// </summary>
	public string Password { get; set; } = null!;

	/// <summary>
	/// Super Admin kullanıcı adı.
	/// </summary>
	public string Username { get; set; } = "superadmin";

	/// <summary>
	/// Super Admin adı.
	/// </summary>
	public string FirstName { get; set; } = "System";

	/// <summary>
	/// Super Admin soyadı.
	/// </summary>
	public string LastName { get; set; } = "Administrator";
}