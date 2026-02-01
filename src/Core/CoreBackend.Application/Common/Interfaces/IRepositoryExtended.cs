using System.Linq.Expressions;
using CoreBackend.Application.Common.Models;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Genişletilmiş repository interface.
/// Temel CRUD + Sayfalama + Dinamik sorgular.
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Id tipi</typeparam>
public interface IRepositoryExtended<TEntity, TId>
	where TEntity : BaseEntity<TId>
	where TId : notnull
{
	#region Basic CRUD

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
	/// Koşula göre ilk entity'yi getirir.
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
	/// Koşula uyan kayıt sayısını döner.
	/// </summary>
	Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Entity ekler.
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
	/// Birden fazla entity günceller.
	/// </summary>
	void UpdateRange(IEnumerable<TEntity> entities);

	/// <summary>
	/// Entity siler.
	/// </summary>
	void Delete(TEntity entity);

	/// <summary>
	/// Birden fazla entity siler.
	/// </summary>
	void DeleteRange(IEnumerable<TEntity> entities);

	#endregion

	#region Pagination & Dynamic Query

	/// <summary>
	/// Sayfalanmış veri getirir.
	/// </summary>
	Task<QueryResult<TEntity>> GetPagedAsync(
		QueryOptions options,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Koşula göre sayfalanmış veri getirir.
	/// </summary>
	Task<QueryResult<TEntity>> GetPagedAsync(
		Expression<Func<TEntity, bool>> predicate,
		QueryOptions options,
		CancellationToken cancellationToken = default);

	#endregion

	#region Include Support

	/// <summary>
	/// İlişkili entity'leri dahil ederek getirir.
	/// </summary>
	Task<TEntity?> GetByIdWithIncludesAsync(
		TId id,
		CancellationToken cancellationToken = default,
		params Expression<Func<TEntity, object>>[] includes);

	/// <summary>
	/// Koşula göre ilişkili entity'leri dahil ederek getirir.
	/// </summary>
	Task<IReadOnlyList<TEntity>> FindWithIncludesAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default,
		params Expression<Func<TEntity, object>>[] includes);

	#endregion

	#region Ignore Filters (Admin Operations)

	/// <summary>
	/// Global filtreleri yoksayarak tüm kayıtları getirir (Admin için).
	/// </summary>
	Task<IReadOnlyList<TEntity>> FindIgnoreFiltersAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Global filtreleri yoksayarak tek kayıt getirir (Admin için).
	/// </summary>
	Task<TEntity?> FirstOrDefaultIgnoreFiltersAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default);

	#endregion

	#region Queryable

	/// <summary>
	/// IQueryable döner (karmaşık sorgular için).
	/// </summary>
	IQueryable<TEntity> AsQueryable();

	/// <summary>
	/// NoTracking IQueryable döner.
	/// </summary>
	IQueryable<TEntity> AsNoTracking();

	#endregion
}