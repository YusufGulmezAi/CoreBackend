namespace CoreBackend.Domain.Common.Interfaces;

/// <summary>
/// Audit (denetim) bilgisi tutan entity'ler için interface.
/// Oluşturma ve güncelleme bilgilerini tutar.
/// </summary>
public interface IAuditable
{
	DateTime CreatedAt { get; }
	Guid? CreatedBy { get; }
	DateTime? ModifiedAt { get; }
	Guid? ModifiedBy { get; }
}