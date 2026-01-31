namespace CoreBackend.Domain.Enums;

/// <summary>
/// Şirket durumları.
/// </summary>
public enum CompanyStatus
{
	/// <summary>
	/// Aktif şirket. Tüm işlemler yapılabilir.
	/// </summary>
	Active = 1,

	/// <summary>
	/// Pasif şirket. Geçici olarak işlem yapılamaz.
	/// </summary>
	Inactive = 2,

	/// <summary>
	/// Kapalı şirket. Tasfiye veya kapanış sürecinde.
	/// </summary>
	Closed = 3,

	/// <summary>
	/// Silinmiş. Soft delete yapılmış şirket.
	/// </summary>
	Deleted = 4
}