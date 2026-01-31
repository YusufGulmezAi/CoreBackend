namespace CoreBackend.Domain.Enums;

/// <summary>
/// Rol seviyesi.
/// Rolün hangi kapsamda geçerli olduğunu belirtir.
/// </summary>
public enum RoleLevel
{
	/// <summary>
	/// Sistem seviyesi. Tüm tenant'lar üzerinde geçerli.
	/// Örnek: Super Admin
	/// </summary>
	System = 1,

	/// <summary>
	/// Tenant seviyesi. Tenant genelinde geçerli.
	/// Örnek: Tenant Admin, Tenant Manager
	/// </summary>
	Tenant = 2,

	/// <summary>
	/// Company seviyesi. Sadece belirli şirkette geçerli.
	/// Örnek: Okul Müdürü, Öğretmen, Veli
	/// </summary>
	Company = 3
}