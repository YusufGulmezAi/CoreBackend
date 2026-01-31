using CoreBackend.Domain.Common.Interfaces;

namespace CoreBackend.Domain.Common.Primitives;

/// <summary>
/// Tenant'a bağlı, Audit ve Soft Delete özellikli base entity.
/// Multi-tenant iş entity'leri bundan türeyecek.
/// </summary>
public abstract class TenantAuditableEntity<TId> : AuditableEntity<TId>, ITenantEntity
	where TId : notnull
{
	public Guid TenantId { get; private set; }

	protected TenantAuditableEntity() : base() { }

	protected TenantAuditableEntity(TId id, Guid tenantId) : base(id)
	{
		TenantId = tenantId;
	}

	public void SetTenantId(Guid tenantId)
	{
		if (TenantId == Guid.Empty)
		{
			TenantId = tenantId;
		}
	}
}