using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Tenant servis implementasyonu.
/// Mevcut tenant bilgisini tutar ve yönetir.
/// </summary>
public class TenantService : ITenantService
{
	private Guid _tenantId;

	/// <summary>
	/// Mevcut Tenant Id.
	/// </summary>
	public Guid TenantId => _tenantId;

	/// <summary>
	/// Tenant Id set edilmiş mi?
	/// </summary>
	public bool HasTenant => _tenantId != Guid.Empty;

	/// <summary>
	/// Tenant Id'yi ayarlar.
	/// </summary>
	public void SetTenantId(Guid tenantId)
	{
		_tenantId = tenantId;
	}

	/// <summary>
	/// Tenant Id'yi temizler.
	/// </summary>
	public void ClearTenant()
	{
		_tenantId = Guid.Empty;
	}
}