using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Infrastructure.Persistence.Context;

namespace CoreBackend.Infrastructure.Persistence.Repositories;

/// <summary>
/// Genişletilmiş repository implementasyonu.
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Id tipi</typeparam>
public class RepositoryExtended<TEntity, TId> : IRepositoryExtended<TEntity, TId>
	where TEntity : BaseEntity<TId>
	where TId : notnull
{
	protected readonly ApplicationDbContext Context;
	protected readonly DbSet<TEntity> DbSet;

	public RepositoryExtended(ApplicationDbContext context)
	{
		Context = context;
		DbSet = context.Set<TEntity>();
	}

	#region Basic CRUD

	public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
	{
		return await DbSet.FindAsync(new object[] { id }, cancellationToken);
	}

	public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
	}

	public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
	}

	public virtual async Task<TEntity?> FirstOrDefaultAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
	}

	public virtual async Task<bool> AnyAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet.AnyAsync(predicate, cancellationToken);
	}

	public virtual async Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
	{
		return predicate == null
			? await DbSet.CountAsync(cancellationToken)
			: await DbSet.CountAsync(predicate, cancellationToken);
	}

	public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
	{
		await DbSet.AddAsync(entity, cancellationToken);
		return entity;
	}

	public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
	{
		await DbSet.AddRangeAsync(entities, cancellationToken);
	}

	public virtual void Update(TEntity entity)
	{
		DbSet.Update(entity);
	}

	public virtual void UpdateRange(IEnumerable<TEntity> entities)
	{
		DbSet.UpdateRange(entities);
	}

	public virtual void Delete(TEntity entity)
	{
		DbSet.Remove(entity);
	}

	public virtual void DeleteRange(IEnumerable<TEntity> entities)
	{
		DbSet.RemoveRange(entities);
	}

	#endregion

	#region Pagination & Dynamic Query

	public virtual async Task<QueryResult<TEntity>> GetPagedAsync(
		QueryOptions options,
		CancellationToken cancellationToken = default)
	{
		var query = DbSet.AsNoTracking().AsQueryable();

		// Search
		if (!string.IsNullOrEmpty(options.SearchText) && options.SearchFields?.Any() == true)
		{
			query = ApplySearch(query, options.SearchText, options.SearchFields);
		}

		// Dynamic Filter
		if (options.Query?.Filters?.Any() == true)
		{
			query = ApplyFilters(query, options.Query.Filters);
		}

		// Count (before pagination)
		var totalCount = await query.CountAsync(cancellationToken);

		// Sort
		if (options.Query?.Sort?.Any() == true)
		{
			query = ApplySort(query, options.Query.Sort);
		}
		else
		{
			// Default sort by CreatedAt descending
			query = ApplyDefaultSort(query);
		}

		// Pagination
		var items = await query
			.Skip((options.PageNumber - 1) * options.PageSize)
			.Take(options.PageSize)
			.ToListAsync(cancellationToken);

		return new QueryResult<TEntity>(items, options.PageNumber, options.PageSize, totalCount);
	}

	public virtual async Task<QueryResult<TEntity>> GetPagedAsync(
		Expression<Func<TEntity, bool>> predicate,
		QueryOptions options,
		CancellationToken cancellationToken = default)
	{
		var query = DbSet.AsNoTracking().Where(predicate);

		// Search
		if (!string.IsNullOrEmpty(options.SearchText) && options.SearchFields?.Any() == true)
		{
			query = ApplySearch(query, options.SearchText, options.SearchFields);
		}

		// Dynamic Filter
		if (options.Query?.Filters?.Any() == true)
		{
			query = ApplyFilters(query, options.Query.Filters);
		}

		// Count (before pagination)
		var totalCount = await query.CountAsync(cancellationToken);

		// Sort
		if (options.Query?.Sort?.Any() == true)
		{
			query = ApplySort(query, options.Query.Sort);
		}
		else
		{
			query = ApplyDefaultSort(query);
		}

		// Pagination
		var items = await query
			.Skip((options.PageNumber - 1) * options.PageSize)
			.Take(options.PageSize)
			.ToListAsync(cancellationToken);

		return new QueryResult<TEntity>(items, options.PageNumber, options.PageSize, totalCount);
	}

	#endregion

	#region Include Support

	public virtual async Task<TEntity?> GetByIdWithIncludesAsync(
		TId id,
		CancellationToken cancellationToken = default,
		params Expression<Func<TEntity, object>>[] includes)
	{
		var query = DbSet.AsQueryable();

		foreach (var include in includes)
		{
			query = query.Include(include);
		}

		return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
	}

	public virtual async Task<IReadOnlyList<TEntity>> FindWithIncludesAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default,
		params Expression<Func<TEntity, object>>[] includes)
	{
		var query = DbSet.AsNoTracking().Where(predicate);

		foreach (var include in includes)
		{
			query = query.Include(include);
		}

		return await query.ToListAsync(cancellationToken);
	}

	#endregion

	#region Ignore Filters (Admin Operations)

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

	public virtual async Task<TEntity?> FirstOrDefaultIgnoreFiltersAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.FirstOrDefaultAsync(predicate, cancellationToken);
	}

	#endregion

	#region Queryable

	public virtual IQueryable<TEntity> AsQueryable()
	{
		return DbSet.AsQueryable();
	}

	public virtual IQueryable<TEntity> AsNoTracking()
	{
		return DbSet.AsNoTracking();
	}

	#endregion

	#region Private Helper Methods

	/// <summary>
	/// Metin arama uygular.
	/// </summary>
	private static IQueryable<TEntity> ApplySearch(
		IQueryable<TEntity> query,
		string searchText,
		List<string> searchFields)
	{
		if (string.IsNullOrWhiteSpace(searchText) || !searchFields.Any())
			return query;

		var parameter = Expression.Parameter(typeof(TEntity), "x");
		Expression? combinedExpression = null;

		foreach (var field in searchFields)
		{
			var property = typeof(TEntity).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (property == null || property.PropertyType != typeof(string))
				continue;

			var propertyAccess = Expression.Property(parameter, property);
			var searchValue = Expression.Constant(searchText.ToLower());

			// property.ToLower().Contains(searchText.ToLower())
			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
			var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

			var toLowerCall = Expression.Call(propertyAccess, toLowerMethod);
			var containsCall = Expression.Call(toLowerCall, containsMethod, searchValue);

			// Null check: property != null && property.ToLower().Contains(...)
			var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
			var safeContains = Expression.AndAlso(nullCheck, containsCall);

			combinedExpression = combinedExpression == null
				? safeContains
				: Expression.OrElse(combinedExpression, safeContains);
		}

		if (combinedExpression == null)
			return query;

		var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
		return query.Where(lambda);
	}

	/// <summary>
	/// Dinamik filtreler uygular.
	/// </summary>
	private static IQueryable<TEntity> ApplyFilters(
		IQueryable<TEntity> query,
		List<FilterDescriptor> filters)
	{
		foreach (var filter in filters)
		{
			var predicate = BuildFilterExpression(filter);
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
		}

		return query;
	}

	/// <summary>
	/// Filtre için expression oluşturur.
	/// </summary>
	private static Expression<Func<TEntity, bool>>? BuildFilterExpression(FilterDescriptor filter)
	{
		var parameter = Expression.Parameter(typeof(TEntity), "x");
		var property = typeof(TEntity).GetProperty(filter.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

		if (property == null)
			return null;

		var propertyAccess = Expression.Property(parameter, property);
		var value = Convert.ChangeType(filter.Value, property.PropertyType);
		var constant = Expression.Constant(value, property.PropertyType);

		Expression comparison = filter.Operator switch
		{
			FilterOperator.Equals => Expression.Equal(propertyAccess, constant),
			FilterOperator.NotEquals => Expression.NotEqual(propertyAccess, constant),
			FilterOperator.GreaterThan => Expression.GreaterThan(propertyAccess, constant),
			FilterOperator.GreaterThanOrEquals => Expression.GreaterThanOrEqual(propertyAccess, constant),
			FilterOperator.LessThan => Expression.LessThan(propertyAccess, constant),
			FilterOperator.LessThanOrEquals => Expression.LessThanOrEqual(propertyAccess, constant),
			FilterOperator.Contains => BuildContainsExpression(propertyAccess, filter.Value?.ToString() ?? ""),
			FilterOperator.StartsWith => BuildStartsWithExpression(propertyAccess, filter.Value?.ToString() ?? ""),
			FilterOperator.EndsWith => BuildEndsWithExpression(propertyAccess, filter.Value?.ToString() ?? ""),
			_ => Expression.Equal(propertyAccess, constant)
		};

		return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
	}

	private static Expression BuildContainsExpression(MemberExpression property, string value)
	{
		var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
		var valueExpr = Expression.Constant(value);
		return Expression.Call(property, method, valueExpr);
	}

	private static Expression BuildStartsWithExpression(MemberExpression property, string value)
	{
		var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
		var valueExpr = Expression.Constant(value);
		return Expression.Call(property, method, valueExpr);
	}

	private static Expression BuildEndsWithExpression(MemberExpression property, string value)
	{
		var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;
		var valueExpr = Expression.Constant(value);
		return Expression.Call(property, method, valueExpr);
	}

	/// <summary>
	/// Dinamik sıralama uygular.
	/// </summary>
	private static IQueryable<TEntity> ApplySort(
		IQueryable<TEntity> query,
		List<SortDescriptor> sorts)
	{
		IOrderedQueryable<TEntity>? orderedQuery = null;

		foreach (var sort in sorts)
		{
			var parameter = Expression.Parameter(typeof(TEntity), "x");
			var property = typeof(TEntity).GetProperty(sort.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
				continue;

			var propertyAccess = Expression.Property(parameter, property);
			var lambda = Expression.Lambda<Func<TEntity, object>>(
				Expression.Convert(propertyAccess, typeof(object)),
				parameter);

			if (orderedQuery == null)
			{
				orderedQuery = sort.Direction == SortDirection.Ascending
					? query.OrderBy(lambda)
					: query.OrderByDescending(lambda);
			}
			else
			{
				orderedQuery = sort.Direction == SortDirection.Ascending
					? orderedQuery.ThenBy(lambda)
					: orderedQuery.ThenByDescending(lambda);
			}
		}

		return orderedQuery ?? query;
	}

	/// <summary>
	/// Varsayılan sıralama uygular (CreatedAt DESC).
	/// </summary>
	private static IQueryable<TEntity> ApplyDefaultSort(IQueryable<TEntity> query)
	{
		var parameter = Expression.Parameter(typeof(TEntity), "x");
		var property = typeof(TEntity).GetProperty("CreatedAt", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

		if (property == null)
		{
			// CreatedAt yoksa Id'ye göre sırala
			property = typeof(TEntity).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (property == null)
				return query;
		}

		var propertyAccess = Expression.Property(parameter, property);
		var lambda = Expression.Lambda<Func<TEntity, object>>(
			Expression.Convert(propertyAccess, typeof(object)),
			parameter);

		return query.OrderByDescending(lambda);
	}

	#endregion
}