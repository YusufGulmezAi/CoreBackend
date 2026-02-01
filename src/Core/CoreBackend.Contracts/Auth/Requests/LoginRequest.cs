namespace CoreBackend.Contracts.Auth.Requests;

/// <summary>
/// Login isteği.
/// </summary>
public class LoginRequest
{
	/// <summary>
	/// Kullanıcı email adresi.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Kullanıcı şifresi.
	/// </summary>
	public string Password { get; set; } = null!;

	/// <summary>
	/// Beni hatırla seçeneği.
	/// true ise session süresi uzar.
	/// </summary>
	public bool RememberMe { get; set; } = false;
}