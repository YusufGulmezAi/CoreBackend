using CoreBackend.Domain.Common.Interfaces;

namespace CoreBackend.Domain.Common.Primitives;

/// <summary>
/// Audit ve Soft Delete özellikli base entity.
/// Çoğu iş entity'si bundan türeyecek.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditable, ISoftDeletable
	where TId : notnull
{
	// IAuditable
	public DateTime CreatedAt { get; private set; }
	public Guid? CreatedBy { get; private set; }
	public DateTime? ModifiedAt { get; private set; }
	public Guid? ModifiedBy { get; private set; }

	// ISoftDeletable
	public bool IsDeleted { get; private set; }
	public DateTime? DeletedAt { get; private set; }
	public Guid? DeletedBy { get; private set; }

	protected AuditableEntity() : base()
	{
		CreatedAt = DateTime.UtcNow;
	}

	protected AuditableEntity(TId id) : base(id)
	{
		CreatedAt = DateTime.UtcNow;
	}

	public void SetCreatedBy(Guid userId)
	{
		CreatedBy = userId;
	}

	public void SetModified(Guid? userId = null)
	{
		ModifiedAt = DateTime.UtcNow;
		ModifiedBy = userId;
	}

	public void Delete(Guid? deletedBy = null)
	{
		IsDeleted = true;
		DeletedAt = DateTime.UtcNow;
		DeletedBy = deletedBy;
	}

	public void Restore()
	{
		IsDeleted = false;
		DeletedAt = null;
		DeletedBy = null;
	}
}