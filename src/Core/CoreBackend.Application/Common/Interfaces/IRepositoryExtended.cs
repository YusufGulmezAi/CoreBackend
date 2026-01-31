using System.Linq.Expressions;
using CoreBackend.Application.Common.Models;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Genişletilmiş repository interface.
/// Dynamic query, pagination, include desteği.
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Primary key tipi</typeparam>
public interface IRepositoryExtended<TEntity, TId> : IRepository<TEntity, TId>
	where TEntity : BaseEntity<TId>
	where TId : notnull
{
	/// <summary>
	/// Sayfalı ve filtrelenmiş veri getirir.
	/// </summary>
	Task<PagedResponse<TEntity>> GetPagedAsync(
		PagedRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sayfalı ve filtrelenmiş veri getirir (ek filtre ile).
	/// </summary>
	Task<PagedResponse<TEntity>> GetPagedAsync(
		PagedRequest request,
		Expression<Func<TEntity, bool>>? additionalFilter,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Dinamik sorgu ile veri getirir.
	/// </summary>
	Task<IReadOnlyList<TEntity>> GetByDynamicQueryAsync(
		DynamicQuery query,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Include ile birlikte Id'ye göre getirir.
	/// </summary>
	Task<TEntity?> GetByIdWithIncludesAsync(
		TId id,
		List<string>? includes = null,
		bool asNoTracking = true,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Include ile birlikte koşula göre getirir.
	/// </summary>
	Task<TEntity?> FirstOrDefaultWithIncludesAsync(
		Expression<Func<TEntity, bool>> predicate,
		List<string>? includes = null,
		bool asNoTracking = true,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Include ile birlikte liste getirir.
	/// </summary>
	Task<IReadOnlyList<TEntity>> FindWithIncludesAsync(
		Expression<Func<TEntity, bool>> predicate,
		List<string>? includes = null,
		List<SortDescriptor>? sorts = null,
		bool asNoTracking = true,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Global filtreleri yoksayarak getirir (Admin için).
	/// </summary>
	Task<TEntity?> GetByIdIgnoreFiltersAsync(
		TId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Global filtreleri yoksayarak liste getirir (Admin için).
	/// </summary>
	Task<IReadOnlyList<TEntity>> FindIgnoreFiltersAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default);
}