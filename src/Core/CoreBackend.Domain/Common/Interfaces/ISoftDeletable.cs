namespace CoreBackend.Domain.Common.Interfaces;

/// <summary>
/// Soft delete özelliği olan entity'ler için interface.
/// Veriler gerçekten silinmez, sadece işaretlenir.
/// </summary>
public interface ISoftDeletable
{
	bool IsDeleted { get; }
	DateTime? DeletedAt { get; }
	Guid? DeletedBy { get; }

	void Delete(Guid? deletedBy = null);
	void Restore();
}