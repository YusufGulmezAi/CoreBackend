using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Errors;
using CoreBackend.Domain.Exceptions;

namespace CoreBackend.Application.Common.Behaviors;

/// <summary>
/// Business rule checker implementation.
/// İş kurallarını kontrol eder, ihlal varsa exception fırlatır.
/// </summary>
public class BusinessRuleChecker : IBusinessRuleChecker
{
	/// <summary>
	/// Tek bir kuralı kontrol eder.
	/// </summary>
	public async Task CheckAsync(IBusinessRule rule, CancellationToken cancellationToken = default)
	{
		if (await rule.IsBrokenAsync(cancellationToken))
		{
			var error = rule.Parameters is not null
				? Error.Create(rule.ErrorCode, rule.ErrorMessage, rule.Parameters)
				: Error.Create(rule.ErrorCode, rule.ErrorMessage);

			throw new DomainException(error);
		}
	}

	/// <summary>
	/// Birden fazla kuralı kontrol eder.
	/// </summary>
	public async Task CheckAllAsync(IEnumerable<IBusinessRule> rules, CancellationToken cancellationToken = default)
	{
		foreach (var rule in rules)
		{
			await CheckAsync(rule, cancellationToken);
		}
	}

	/// <summary>
	/// Birden fazla kuralı kontrol eder (params ile).
	/// </summary>
	public async Task CheckAllAsync(CancellationToken cancellationToken = default, params IBusinessRule[] rules)
	{
		await CheckAllAsync(rules, cancellationToken);
	}
}