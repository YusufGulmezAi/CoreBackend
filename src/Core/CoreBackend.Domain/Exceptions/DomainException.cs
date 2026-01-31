using CoreBackend.Domain.Errors;

namespace CoreBackend.Domain.Exceptions;

/// <summary>
/// Domain katmanı için temel exception sınıfı.
/// Tüm custom exception'lar bundan türer.
/// </summary>
public class DomainException : Exception
{
	public Error Error { get; }

	public DomainException(Error error)
		: base(error.Message)
	{
		Error = error;
	}

	public DomainException(string code, string message)
		: base(message)
	{
		Error = Error.Create(code, message);
	}

	public DomainException(string code, string message, object parameters)
		: base(message)
	{
		Error = Error.Create(code, message, parameters);
	}

	public DomainException(string code, string message, Exception innerException)
		: base(message, innerException)
	{
		Error = Error.Create(code, message);
	}
}