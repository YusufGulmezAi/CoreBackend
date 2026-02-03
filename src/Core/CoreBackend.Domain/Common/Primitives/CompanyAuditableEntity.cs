using CoreBackend.Domain.Common.Interfaces;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Domain.Common.Primitives;

/// <summary>
/// Şirkete bağlı, Tenant'a bağlı, Audit ve Soft Delete özellikli base entity.
/// Fatura, hesap, işlem gibi şirket bazlı entity'ler bundan türeyecek.
/// </summary>
public abstract class CompanyAuditableEntity<TId> : TenantAuditableEntity<TId>, ICompanyEntity
	where TId : notnull
{
	public Guid CompanyId { get; private set; }

	/// <summary>
	/// Şirket.
	/// </summary>
	public Company Company { get; private set; }

	protected CompanyAuditableEntity() : base() { }

	protected CompanyAuditableEntity(TId id, Guid tenantId, Guid companyId) : base(id, tenantId)
	{
		CompanyId = companyId;
	}

	public void SetCompanyId(Guid companyId)
	{
		if (CompanyId == Guid.Empty)
		{
			CompanyId = companyId;
		}
	}
}