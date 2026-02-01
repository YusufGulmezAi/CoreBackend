namespace CoreBackend.Domain.Enums;

/// <summary>
/// İki faktörlü doğrulama metodları.
/// </summary>
public enum TwoFactorMethod
{
	/// <summary>
	/// Devre dışı.
	/// </summary>
	None = 0,

	/// <summary>
	/// Email ile kod.
	/// </summary>
	Email = 1,

	/// <summary>
	/// TOTP (Google Authenticator, Authy vb.)
	/// </summary>
	Totp = 2,

	/// <summary>
	/// SMS ile kod.
	/// </summary>
	Sms = 3
}