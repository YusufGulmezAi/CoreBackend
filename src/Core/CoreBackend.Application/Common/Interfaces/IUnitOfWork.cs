using CoreBackend.Application.Common.Models;
using CoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Unit of Work interface.
/// Hybrid yaklaşım: Doğrudan DbSet erişimi + Helper metodlar.
/// </summary>
public interface IUnitOfWork : IDisposable
{
	#region DbSets - Doğrudan Erişim

	DbSet<Tenant> Tenants { get; }
	DbSet<Company> Companies { get; }
	DbSet<User> Users { get; }
	DbSet<Role> Roles { get; }
	DbSet<Permission> Permissions { get; }
	DbSet<UserRole> UserRoles { get; }
	DbSet<UserCompanyRole> UserCompanyRoles { get; }
	DbSet<RolePermission> RolePermissions { get; }
	DbSet<UserSession> UserSessions { get; }
	DbSet<SessionHistory> SessionHistories { get; }
	DbSet<TwoFactorCode> TwoFactorCodes { get; }

	#endregion

	#region Query Helpers

	/// <summary>
	/// Herhangi bir entity için IQueryable döner.
	/// </summary>
	IQueryable<T> Query<T>() where T : class;

	/// <summary>
	/// NoTracking IQueryable döner (read-only sorgular için).
	/// </summary>
	IQueryable<T> QueryNoTracking<T>() where T : class;

	/// <summary>
	/// Global filtreleri yoksayarak sorgu yapar.
	/// </summary>
	IQueryable<T> QueryIgnoreFilters<T>() where T : class;

	#endregion

	#region Pagination

	/// <summary>
	/// Sayfalanmış sonuç döner.
	/// </summary>
	Task<QueryResult<T>> GetPagedAsync<T>(
		IQueryable<T> query,
		QueryOptions options,
		CancellationToken cancellationToken = default) where T : class;


	#endregion

	#region Transaction

	/// <summary>
	/// Değişiklikleri kaydeder.
	/// </summary>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Transaction başlatır.
	/// </summary>
	Task BeginTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Transaction'ı commit eder.
	/// </summary>
	Task CommitTransactionAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Transaction'ı geri alır.
	/// </summary>
	Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

	#endregion
}