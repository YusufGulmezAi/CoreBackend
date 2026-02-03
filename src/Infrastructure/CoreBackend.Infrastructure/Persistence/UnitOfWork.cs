using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Domain.Entities;
using CoreBackend.Infrastructure.Persistence.Context;
using CoreBackend.Infrastructure.Persistence.Extensions;

namespace CoreBackend.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementasyonu.
/// Hybrid yaklaşım: Doğrudan DbSet erişimi + Helper metodlar.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _context;
	private IDbContextTransaction? _transaction;
	private bool _disposed;

	public UnitOfWork(ApplicationDbContext context)
	{
		_context = context;
	}

	#region DbSets

	public DbSet<Tenant> Tenants => _context.Tenants;
	public DbSet<Company> Companies => _context.Companies;
	public DbSet<User> Users => _context.Users;
	public DbSet<Role> Roles => _context.Roles;
	public DbSet<Permission> Permissions => _context.Permissions;
	public DbSet<UserRole> UserRoles => _context.UserRoles;
	public DbSet<UserCompanyRole> UserCompanyRoles => _context.UserCompanyRoles;
	public DbSet<RolePermission> RolePermissions => _context.RolePermissions;
	public DbSet<UserSession> UserSessions => _context.UserSessions;
	public DbSet<SessionHistory> SessionHistories => _context.SessionHistories;
	public DbSet<TwoFactorCode> TwoFactorCodes => _context.TwoFactorCodes;

	#endregion

	#region Query Helpers

	public IQueryable<T> Query<T>() where T : class
		=> _context.Set<T>().AsQueryable();

	public IQueryable<T> QueryNoTracking<T>() where T : class
		=> _context.Set<T>().AsNoTracking();

	public IQueryable<T> QueryIgnoreFilters<T>() where T : class
		=> _context.Set<T>().IgnoreQueryFilters().AsNoTracking();

	#endregion

	#region Pagination

	public async Task<QueryResult<T>> GetPagedAsync<T>(
		IQueryable<T> query,
		QueryOptions options,
		CancellationToken cancellationToken = default) where T : class
	{
		// Search
		query = query.ApplySearch(options.SearchText, options.SearchFields);

		// Filters
		if (options.Query != null)
		{
			query = query.ApplyFilters(options.Query.Filters);
			query = query.ApplyFilterGroups(options.Query.FilterGroups);
		}

		// Count (before pagination)
		var totalCount = await query.CountAsync(cancellationToken);

		// Sort
		if (options.Query?.HasSort == true)
		{
			query = query.ApplySort(options.Query.Sort);
		}
		else
		{
			query = query.ApplyDefaultSort();
		}

		// Pagination
		var items = await query
			.ApplyPaging(options.PageNumber, options.PageSize)
			.ToListAsync(cancellationToken);

		return new QueryResult<T>(items, options.PageNumber, options.PageSize, totalCount);
	}

	#endregion

	#region Transaction

	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		=> await _context.SaveChangesAsync(cancellationToken);

	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		_transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
	}

	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction != null)
		{
			await _transaction.CommitAsync(cancellationToken);
			await _transaction.DisposeAsync();
			_transaction = null;
		}
	}

	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction != null)
		{
			await _transaction.RollbackAsync(cancellationToken);
			await _transaction.DisposeAsync();
			_transaction = null;
		}
	}

	#endregion

	#region Dispose

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_transaction?.Dispose();
			_context.Dispose();
		}
		_disposed = true;
	}

	#endregion
}