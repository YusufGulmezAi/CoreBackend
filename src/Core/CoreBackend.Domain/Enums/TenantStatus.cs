namespace CoreBackend.Domain.Enums;

/// <summary>
/// Tenant (Mali Müşavir/Muhasebeci) hesap durumları.
/// </summary>
public enum TenantStatus
{
	/// <summary>
	/// Aktif hesap. Tüm özellikler kullanılabilir.
	/// </summary>
	Active = 1,

	/// <summary>
	/// Pasif hesap. Kullanıcı tarafından devre dışı bırakılmış.
	/// </summary>
	Inactive = 2,

	/// <summary>
	/// Askıya alınmış. Ödeme sorunu veya kural ihlali nedeniyle.
	/// </summary>
	Suspended = 3,

	/// <summary>
	/// Deneme sürümü. Sınırlı özellikler.
	/// </summary>
	Trial = 4,

	/// <summary>
	/// Silinmiş. Soft delete yapılmış hesap.
	/// </summary>
	Deleted = 5
}