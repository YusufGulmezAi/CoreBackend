using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Infrastructure.Persistence.Context;

namespace CoreBackend.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementasyonu.
/// Transaction yönetimi ve değişikliklerin kaydedilmesi.
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

	/// <summary>
	/// Tüm değişiklikleri veritabanına kaydeder.
	/// </summary>
	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await _context.SaveChangesAsync(cancellationToken);
	}

	/// <summary>
	/// Transaction başlatır.
	/// </summary>
	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction != null)
		{
			return; // Zaten aktif transaction var
		}

		_transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
	}

	/// <summary>
	/// Transaction'ı onaylar.
	/// </summary>
	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction == null)
		{
			return;
		}

		try
		{
			await _context.SaveChangesAsync(cancellationToken);
			await _transaction.CommitAsync(cancellationToken);
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	/// <summary>
	/// Transaction'ı geri alır.
	/// </summary>
	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_transaction == null)
		{
			return;
		}

		try
		{
			await _transaction.RollbackAsync(cancellationToken);
		}
		finally
		{
			await DisposeTransactionAsync();
		}
	}

	/// <summary>
	/// Transaction'ı dispose eder.
	/// </summary>
	private async Task DisposeTransactionAsync()
	{
		if (_transaction != null)
		{
			await _transaction.DisposeAsync();
			_transaction = null;
		}
	}

	/// <summary>
	/// Dispose pattern.
	/// </summary>
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
}