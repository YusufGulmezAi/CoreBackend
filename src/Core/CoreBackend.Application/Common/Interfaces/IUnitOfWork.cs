namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface.
/// Transaction yönetimi ve değişikliklerin kaydedilmesi için kullanılır.
/// </summary>
public interface IUnitOfWork : IDisposable
{
	/// <summary>
	/// Tüm değişiklikleri veritabanına kaydeder.
	/// </summary>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Transaction başlatır.
	/// </summary>
	Task BeginTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Transaction'ı onaylar.
	/// </summary>
	Task CommitTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Transaction'ı geri alır.
	/// </summary>
	Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}