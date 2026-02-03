using MediatR;
using Microsoft.Extensions.Logging;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Application.Common.Behaviors;

/// <summary>
/// Transaction behavior.
/// Command'ları transaction içinde çalıştırır.
/// Execution strategy ile uyumlu çalışır.
/// </summary>
/// <typeparam name="TRequest">Request tipi</typeparam>
/// <typeparam name="TResponse">Response tipi</typeparam>
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
		if (typeof(TRequest).Name.EndsWith("Query"))
			return await next();

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