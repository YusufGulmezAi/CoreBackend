namespace CoreBackend.Domain.Enums;

/// <summary>
/// Tenant 2FA politikaları.
/// </summary>
public enum TwoFactorPolicy
{
	/// <summary>
	/// Kullanıcı isteğine bağlı.
	/// </summary>
	Optional = 0,

	/// <summary>
	/// Tüm kullanıcılar için zorunlu.
	/// </summary>
	Required = 1,

	/// <summary>
	/// Sadece adminler için zorunlu.
	/// </summary>
	RequiredForAdmins = 2
}