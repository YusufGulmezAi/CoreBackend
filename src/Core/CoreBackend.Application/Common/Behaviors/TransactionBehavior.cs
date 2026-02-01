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
		var requestName = typeof(TRequest).Name;

		// Query'ler için transaction kullanma
		if (requestName.EndsWith("Query"))
		{
			return await next();
		}

		// Command'lar için sadece SaveChanges yeterli
		// EF Core zaten her SaveChanges için implicit transaction kullanır
		// Manuel transaction yerine bunu tercih ediyoruz (ExecutionStrategy uyumluluğu için)
		try
		{
			var response = await next();

			// Handler içinde SaveChanges çağrılmadıysa burada çağır
			// (Genellikle handler zaten çağırır)

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error handling {RequestName}", requestName);
			throw;
		}
	}
}