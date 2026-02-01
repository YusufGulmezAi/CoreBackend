namespace CoreBackend.Domain.Enums;

/// <summary>
/// Session aksiyonları.
/// </summary>
public enum SessionAction
{
	/// <summary>
	/// Kullanıcı girişi.
	/// </summary>
	Login = 1,

	/// <summary>
	/// Kullanıcı çıkışı.
	/// </summary>
	Logout = 2,

	/// <summary>
	/// Token yenileme.
	/// </summary>
	TokenRefresh = 3,

	/// <summary>
	/// Admin tarafından iptal.
	/// </summary>
	RevokedByAdmin = 4,

	/// <summary>
	/// Süre dolumu.
	/// </summary>
	Expired = 5,

	/// <summary>
	/// Şüpheli aktivite nedeniyle iptal.
	/// </summary>
	RevokedSuspicious = 6,

	/// <summary>
	/// Şifre değişikliği nedeniyle iptal.
	/// </summary>
	RevokedPasswordChange = 7,

	/// <summary>
	/// Tüm cihazlardan çıkış.
	/// </summary>
	LogoutAll = 8,

	/// <summary>
	/// 2FA doğrulaması.
	/// </summary>
	TwoFactorVerified = 9,

	/// <summary>
	/// 2FA başarısız deneme.
	/// </summary>
	TwoFactorFailed = 10
}