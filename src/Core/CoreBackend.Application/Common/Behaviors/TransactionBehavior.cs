using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Application.Common.Behaviors;

/// <summary>
/// Transaction behavior.
/// Command'ları transaction içinde çalıştırır.
/// Execution strategy ile uyumlu çalışır.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

	public TransactionBehavior(
		IUnitOfWork unitOfWork,
		ILogger<TransactionBehavior<TRequest, TResponse>> logger)
	{
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		// Query'ler için transaction kullanma
		if (typeof(TRequest).Name.EndsWith("Query"))
			return await next();

		// Login gibi bazı command'lar için de transaction gerekli olmayabilir
		// İsteğe bağlı: Transaction gerektiren command'ları marker interface ile belirle
		var requiresTransaction = typeof(TRequest).GetInterfaces()
			.Any(i => i.Name == "ITransactionalRequest");

		if (!requiresTransaction)
		{
			// Transaction olmadan çalıştır, sadece SaveChanges yap
			var response = await next();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return response;
		}

		// Transaction ile çalıştır
		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var response = await next();
			await _unitOfWork.CommitTransactionAsync(cancellationToken);
			return response;
		}
		catch
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			throw;
		}
	}
}