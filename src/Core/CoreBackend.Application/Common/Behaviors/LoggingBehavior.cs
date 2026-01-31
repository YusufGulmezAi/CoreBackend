using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR logging behavior.
/// Her request'in başlangıç, bitiş ve süresini loglar.
/// </summary>
/// <typeparam name="TRequest">Request tipi</typeparam>
/// <typeparam name="TResponse">Response tipi</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
	private readonly ICurrentUserService _currentUserService;

	public LoggingBehavior(
		ILogger<LoggingBehavior<TRequest, TResponse>> logger,
		ICurrentUserService currentUserService)
	{
		_logger = logger;
		_currentUserService = currentUserService;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var requestName = typeof(TRequest).Name;
		var userId = _currentUserService.UserId;
		var tenantId = _currentUserService.TenantId;

		_logger.LogInformation(
			"Handling {RequestName} | UserId: {UserId} | TenantId: {TenantId}",
			requestName,
			userId,
			tenantId);

		var stopwatch = Stopwatch.StartNew();

		try
		{
			var response = await next();

			stopwatch.Stop();

			_logger.LogInformation(
				"Handled {RequestName} | Duration: {Duration}ms | UserId: {UserId}",
				requestName,
				stopwatch.ElapsedMilliseconds,
				userId);

			// Yavaş sorguları uyar (500ms üzeri)
			if (stopwatch.ElapsedMilliseconds > 500)
			{
				_logger.LogWarning(
					"Long running request: {RequestName} | Duration: {Duration}ms | UserId: {UserId}",
					requestName,
					stopwatch.ElapsedMilliseconds,
					userId);
			}

			return response;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			_logger.LogError(
				ex,
				"Error handling {RequestName} | Duration: {Duration}ms | UserId: {UserId} | Error: {ErrorMessage}",
				requestName,
				stopwatch.ElapsedMilliseconds,
				userId,
				ex.Message);

			throw;
		}
	}
}