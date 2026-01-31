using FluentValidation;
using MediatR;
using DomainValidationException = CoreBackend.Domain.Exceptions.ValidationException;

namespace CoreBackend.Application.Common.Behaviors;

/// <summary>
/// MediatR validation behavior.
/// Her request'ten önce FluentValidation kurallarını çalıştırır.
/// </summary>
/// <typeparam name="TRequest">Request tipi</typeparam>
/// <typeparam name="TResponse">Response tipi</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		// Validator yoksa devam et
		if (!_validators.Any())
		{
			return await next();
		}

		// Validation context oluştur
		var context = new ValidationContext<TRequest>(request);

		// Tüm validator'ları çalıştır
		var validationResults = await Task.WhenAll(
			_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

		// Hataları topla
		var failures = validationResults
			.Where(r => r.Errors.Any())
			.SelectMany(r => r.Errors)
			.GroupBy(f => f.PropertyName)
			.ToDictionary(
				g => g.Key,
				g => g.Select(f => f.ErrorCode ?? f.ErrorMessage).ToArray());

		// Hata varsa exception fırlat
		if (failures.Any())
		{
			throw new DomainValidationException(failures);
		}

		return await next();
	}
}