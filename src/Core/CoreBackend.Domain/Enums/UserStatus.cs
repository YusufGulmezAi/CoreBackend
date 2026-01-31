namespace CoreBackend.Domain.Enums;

/// <summary>
/// Kullanıcı hesap durumları.
/// </summary>
public enum UserStatus
{
	/// <summary>
	/// Aktif kullanıcı. Sisteme giriş yapabilir.
	/// </summary>
	Active = 1,

	/// <summary>
	/// Pasif kullanıcı. Giriş yapamaz.
	/// </summary>
	Inactive = 2,

	/// <summary>
	/// Kilitli hesap. Çok fazla başarısız giriş denemesi.
	/// </summary>
	Locked = 3,

	/// <summary>
	/// Email doğrulaması bekliyor.
	/// </summary>
	PendingVerification = 4,

	/// <summary>
	/// Silinmiş kullanıcı. Soft delete yapılmış.
	/// </summary>
	Deleted = 5
}