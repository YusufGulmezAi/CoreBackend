namespace CoreBackend.Contracts.Auth.Requests;

/// <summary>
/// Kayıt isteği.
/// </summary>
public class RegisterRequest
{
	/// <summary>
	/// Tenant adı.
	/// </summary>
	public string TenantName { get; set; } = null!;

	/// <summary>
	/// Kullanıcı adı.
	/// </summary>
	public string Username { get; set; } = null!;

	/// <summary>
	/// Email adresi.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Şifre.
	/// </summary>
	public string Password { get; set; } = null!;

	/// <summary>
	/// Şifre tekrarı.
	/// </summary>
	public string ConfirmPassword { get; set; } = null!;

	/// <summary>
	/// Ad.
	/// </summary>
	public string FirstName { get; set; } = null!;

	/// <summary>
	/// Soyad.
	/// </summary>
	public string LastName { get; set; } = null!;

	/// <summary>
	/// Telefon numarası (opsiyonel).
	/// </summary>
	public string? Phone { get; set; }
}