using MediatR;
using Microsoft.Extensions.Logging;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR transaction behavior.
/// Command'lar için otomatik transaction yönetimi sağlar.
/// </summary>
/// <typeparam name="TRequest">Request tipi</typeparam>
/// <typeparam name="TResponse">Response tipi</typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
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
		var requestName = typeof(TRequest).Name;

		// Sadece Command'lar için transaction başlat
		// Query'ler "Query" ile biter, Command'lar "Command" ile biter
		if (requestName.EndsWith("Query"))
		{
			return await next();
		}

		_logger.LogDebug("Beginning transaction for {RequestName}", requestName);

		try
		{
			await _unitOfWork.BeginTransactionAsync(cancellationToken);

			var response = await next();

			await _unitOfWork.CommitTransactionAsync(cancellationToken);

			_logger.LogDebug("Transaction committed for {RequestName}", requestName);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Transaction rolled back for {RequestName}", requestName);

			await _unitOfWork.RollbackTransactionAsync(cancellationToken);

			throw;
		}
	}
}