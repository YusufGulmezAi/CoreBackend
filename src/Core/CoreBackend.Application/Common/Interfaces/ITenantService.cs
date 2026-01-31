namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Tenant servis interface.
/// Mevcut tenant bilgisine erişim sağlar.
/// </summary>
public interface ITenantService
{
	/// <summary>
	/// Mevcut Tenant Id.
	/// </summary>
	Guid TenantId { get; }

	/// <summary>
	/// Tenant Id set edilmiş mi?
	/// </summary>
	bool HasTenant { get; }

	/// <summary>
	/// Tenant Id'yi ayarlar.
	/// </summary>
	void SetTenantId(Guid tenantId);

	/// <summary>
	/// Tenant Id'yi temizler.
	/// </summary>
	void ClearTenant();
}