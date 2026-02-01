using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Infrastructure.Persistence.Context;

namespace CoreBackend.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementasyonu.
/// </summary>
/// <typeparam name="TEntity">Entity tipi</typeparam>
/// <typeparam name="TId">Primary key tipi</typeparam>
public class Repository<TEntity, TId> : IRepository<TEntity, TId>
	where TEntity : BaseEntity<TId>
	where TId : notnull
{
	protected readonly ApplicationDbContext Context;
	protected readonly DbSet<TEntity> DbSet;

	public Repository(ApplicationDbContext context)
	{
		Context = context;
		DbSet = context.Set<TEntity>();
	}

	/// <summary>
	/// Id ile entity getirir.
	/// </summary>
	public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
	{
		return await DbSet.FindAsync(new object[] { id }, cancellationToken);
	}

	/// <summary>
	/// Tüm entity'leri getirir.
	/// </summary>
	public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await DbSet.ToListAsync(cancellationToken);
	}

	/// <summary>
	/// Koşula göre entity'leri getirir.
	/// </summary>
	public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet.Where(predicate).ToListAsync(cancellationToken);
	}

	/// <summary>
	/// Koşula göre tek entity getirir.
	/// </summary>
	public virtual async Task<TEntity?> FirstOrDefaultAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
	}

	/// <summary>
	/// Koşula uyan kayıt var mı kontrol eder.
	/// </summary>
	public virtual async Task<bool> AnyAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet.AnyAsync(predicate, cancellationToken);
	}

	/// <summary>
	/// Koşula uyan kayıt sayısını getirir.
	/// </summary>
	public virtual async Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		CancellationToken cancellationToken = default)
	{
		return predicate == null
			? await DbSet.CountAsync(cancellationToken)
			: await DbSet.CountAsync(predicate, cancellationToken);
	}

	/// <summary>
	/// Yeni entity ekler.
	/// </summary>
	public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
	{
		await DbSet.AddAsync(entity, cancellationToken);
		return entity;
	}

	/// <summary>
	/// Birden fazla entity ekler.
	/// </summary>
	public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
	{
		await DbSet.AddRangeAsync(entities, cancellationToken);
	}

	/// <summary>
	/// Global filtreleri yoksayarak tek kayıt getirir (Admin için).
	/// </summary>
	public virtual async Task<TEntity?> FirstOrDefaultIgnoreFiltersAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken cancellationToken = default)
	{
		return await DbSet
			.IgnoreQueryFilters()
			.AsNoTracking()
			.FirstOrDefaultAsync(predicate, cancellationToken);
	}

	/// <summary>
	/// Entity günceller.
	/// </summary>
	public virtual void Update(TEntity entity)
	{
		DbSet.Update(entity);
	}

	/// <summary>
	/// Entity siler.
	/// </summary>
	public virtual void Remove(TEntity entity)
	{
		DbSet.Remove(entity);
	}

	/// <summary>
	/// Birden fazla entity siler.
	/// </summary>
	public virtual void RemoveRange(IEnumerable<TEntity> entities)
	{
		DbSet.RemoveRange(entities);
	}

	/// <summary>
	/// Global filtreleri yoksayarak tek kayıt getirir (Admin için).
	/// </summary>
	
}