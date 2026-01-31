namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Business rule checker interface.
/// İş kurallarını kontrol eder, ihlal varsa exception fırlatır.
/// </summary>
public interface IBusinessRuleChecker
{
	/// <summary>
	/// Tek bir kuralı kontrol eder.
	/// </summary>
	Task CheckAsync(IBusinessRule rule, CancellationToken cancellationToken = default);

	/// <summary>
	/// Birden fazla kuralı kontrol eder.
	/// </summary>
	Task CheckAllAsync(IEnumerable<IBusinessRule> rules, CancellationToken cancellationToken = default);

	/// <summary>
	/// Birden fazla kuralı kontrol eder (params ile).
	/// </summary>
	Task CheckAllAsync(CancellationToken cancellationToken = default, params IBusinessRule[] rules);
}