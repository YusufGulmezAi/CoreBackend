using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Infrastructure.Persistence.Context;
using CoreBackend.Infrastructure.Persistence.Extensions;

namespace CoreBackend.Infrastructure.Persistence.Repositories;

/// <summary>
/// Genişletilmiş repository implementasyonu.
/// Dynamic query, pagination, include desteği.
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Primary key tipi</typeparam>
public class RepositoryExtended<TEntity, TId> : Repository<TEntity, TId>, IRepositoryExtended<TEntity, TId>
	where TEntity : BaseEntity<TId>
	where TId : notnull
{
	public RepositoryExtended(ApplicationDbContext context) : base(context)
	{
	}

	/// <summary>
	/// Sayfalı ve filtrelenmiş veri getirir.
	/// </summary>
	public virtual async Task<PagedResponse<TEntity>> GetPagedAsync(
		PagedRequest request,
		CancellationToken cancellationToken = default)
	{
		return await GetPagedAsync(request, null, cancellationToken);
	}

	/// <summary>
	/// Sayfalı ve filtrelenmiş veri getirir (ek filtre ile).
	/// </summary>
	public virtual async Task<PagedResponse<TEntity>> GetPagedAsync(
		PagedRequest request,
		Expression<Func<TEntity, bool>>? additionalFilter,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = DbSet;

		// Global filtreleri yoksay (Admin için)
		if (request.Query?.IgnoreQueryFilters == true)
		{
			query = query.IgnoreQueryFilters();
		}

		// AsNoTracking
		if (request.Query?.AsNoTracking != false)
		{
			query = query.AsNoTracking();
		}

		// Ek filtre uygula
		if (additionalFilter != null)
		{
			query = query.Where(additionalFilter);
		}

		// Hızlı arama
		if (!string.IsNullOrWhiteSpace(request.SearchText) && request.SearchFields.Any())
		{
			query = query.ApplySearch(request.SearchText, request.SearchFields);
		}

		// Dinamik sorgu (filter, includes)
		if (request.Query != null)
		{
			// Includes
			if (request.Query.Includes.Any())
			{
				query = query.ApplyIncludes(request.Query.Includes);
			}

			// Filters
			if (request.Query.Filter != null)
			{
				query = query.ApplyFilterGroup(request.Query.Filter);
			}
		}

		// Toplam kayıt sayısı
		var totalCount = await query.CountAsync(cancellationToken);

		// Sıralama
		if (request.Query?.Sort.Any() == true)
		{
			query = query.ApplySorting(request.Query.Sort);
		}

		// Sayfalama
		var items = await query
			.ApplyPaging(request.PageNumber, request.PageSize)
			.ToListAsync(cancellationToken);

		return PagedResponse<TEntity>.Create(items, request.PageNumber, request.PageSize, totalCount);
	}

	/// <summary>
	/// Dinamik sorgu ile veri getirir.
	/// </summary>
	public virtual async Task<IReadOnlyList<TEntity>> GetByDynamicQueryAsync(
		DynamicQuery query,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> dbQuery = DbSet;

		// Global filtreleri yoksay
		if (query.IgnoreQueryFilters)
		{
			dbQuery = dbQuery.IgnoreQueryFilters();
		}

		// Dinamik sorgu uygula
		dbQuery = dbQuery.ApplyDynamicQuery(query);

		return await dbQuery.ToListAsync(cancellationToken);
	}

	/// <summary>
	/// Include ile birlikte Id'ye göre getirir.
	/// </summary>
	public virtual async Task<TEntity?> GetByIdWithIncludesAsync(
		TId id,
		List<string>? includes = null,
		bool asNoTracking = true,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = DbSet;

		if (asNoTracking)
		{
			query = query.AsNoTracking();
		}

		if (includes != null && includes.Any())
		{
			query = query.ApplyIncludes(includes);
		}

		return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
	}

	/// <summary>
	/// Include ile birlikte koşula göre getirir.
	/// </summary>
	public virtual async Task<TEntity?> FirstOrDefaultWithIncludesAsync(
		Expression<Func<TEntity, bool>> predicate,
		List<string>? includes = null,
		bool asNoTracking = true,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = DbSet;

		if (asNoTracking)
		{
			query = query.AsNoTracking();
		}

		if (includes != null && includes.Any())
		{
			query = query.ApplyIncludes(includes);
		}

		return await query.FirstOrDefaultAsync(predicate, cancellationToken);
	}

	/// <summary>
	/// Include ile birlikte liste getirir.
	/// </summary>
	public virtual async Task<IReadOnlyList<TEntity>> FindWithIncludesAsync(
		Expression<Func<TEntity, bool>> predicate,
		List<string>? includes = null,
		List<SortDescriptor>? sorts = null,
		bool asNoTracking = true,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = DbSet;

		if (asNoTracking)
		{
			query = query.AsNoTracking();
		}

		if (includes != null && includes.Any())
		{
			query = query.ApplyIncludes(includes);
		}

		query = query.Where(predicate);

		if (sorts != null && sorts.Any())
		{
			query = query.ApplySorting(sorts);
		}

		return await query.ToListAsync(cancellationToken);
	}

	/// <summary>
	/// Global filtreleri yoksayarak getirir (Admin için).
	/// </summary>
	public virtual async Task<TEntity?> GetByIdIgnoreFiltersAsync(
		TId id,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
	}

	/// <summary>
	/// Global filtreleri yoksayarak liste getirir (Admin için).
	/// </summary>
	public virtual async Task<IReadOnlyList<TEntity>> FindIgnoreFiltersAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.Where(predicate)
			.ToListAsync(cancellationToken);
	}
}