namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Business rule abstract class.
/// Sync kurallar için temel sınıf.
/// </summary>
public abstract class BusinessRule : IBusinessRule
{
	public abstract string ErrorCode { get; }
	public abstract string ErrorMessage { get; }
	public virtual object? Parameters => null;

	/// <summary>
	/// Sync kural kontrolü.
	/// </summary>
	public abstract bool IsBroken();

	/// <summary>
	/// Async wrapper.
	/// </summary>
	public virtual Task<bool> IsBrokenAsync(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(IsBroken());
	}
}

/// <summary>
/// Async business rule abstract class.
/// Veritabanı kontrolü gerektiren kurallar için.
/// </summary>
public abstract class AsyncBusinessRule : IBusinessRule
{
	public abstract string ErrorCode { get; }
	public abstract string ErrorMessage { get; }
	public virtual object? Parameters => null;

	/// <summary>
	/// Async kural kontrolü.
	/// </summary>
	public abstract Task<bool> IsBrokenAsync(CancellationToken cancellationToken = default);
}