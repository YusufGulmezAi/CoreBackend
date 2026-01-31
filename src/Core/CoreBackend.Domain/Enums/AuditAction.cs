namespace CoreBackend.Domain.Enums;

/// <summary>
/// Denetim (Audit) işlem türleri.
/// Log ve tarihçe kayıtlarında kullanılır.
/// </summary>
public enum AuditAction
{
	/// <summary>
	/// Yeni kayıt oluşturuldu.
	/// </summary>
	Created = 1,

	/// <summary>
	/// Kayıt güncellendi.
	/// </summary>
	Updated = 2,

	/// <summary>
	/// Kayıt silindi (soft delete).
	/// </summary>
	Deleted = 3,

	/// <summary>
	/// Silinen kayıt geri yüklendi.
	/// </summary>
	Restored = 4,

	/// <summary>
	/// Kayıt kalıcı olarak silindi (hard delete).
	/// </summary>
	PermanentlyDeleted = 5
}