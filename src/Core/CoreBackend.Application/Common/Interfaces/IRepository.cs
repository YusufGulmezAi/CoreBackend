using System.Linq.Expressions;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface.
/// Tüm entity'ler için temel CRUD operasyonlarını tanımlar.
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Primary key tipi</typeparam>
public interface IRepository<TEntity, TId>
	where TEntity : BaseEntity<TId>
	where TId : notnull
{
	/// <summary>
	/// Id ile entity getirir.
	/// </summary>
	Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

	/// <summary>
	/// Tüm entity'leri getirir.
	/// </summary>
	Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Koşula göre entity'leri getirir.
	/// </summary>
	Task<IReadOnlyList<TEntity>> FindAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Koşula göre tek entity getirir.
	/// </summary>
	Task<TEntity?> FirstOrDefaultAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Koşula uyan kayıt var mı kontrol eder.
	/// </summary>
	Task<bool> AnyAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Koşula uyan kayıt sayısını getirir.
	/// </summary>
	Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Yeni entity ekler.
	/// </summary>
	Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

	/// <summary>
	/// Birden fazla entity ekler.
	/// </summary>
	Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

	/// <summary>
	/// Entity günceller.
	/// </summary>
	void Update(TEntity entity);

	/// <summary>
	/// Entity siler.
	/// </summary>
	void Remove(TEntity entity);

	/// <summary>
	/// Birden fazla entity siler.
	/// </summary>
	void RemoveRange(IEnumerable<TEntity> entities);
}