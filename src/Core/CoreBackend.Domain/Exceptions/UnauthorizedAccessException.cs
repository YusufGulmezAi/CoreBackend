using CoreBackend.Domain.Errors;

namespace CoreBackend.Domain.Exceptions;

/// <summary>
/// Yetkisiz erişim denemelerinde fırlatılır.
/// HTTP 401 Unauthorized veya 403 Forbidden ile eşleşir.
/// </summary>
public class UnauthorizedAccessException : DomainException
{
	public UnauthorizedAccessException()
		: base(Error.Create(
			ErrorCodes.Auth.UnauthorizedAccess,
			"Unauthorized access attempt."))
	{
	}

	public UnauthorizedAccessException(string errorCode)
		: base(Error.Create(
			errorCode,
			"Unauthorized access attempt.",
			new { errorCode }))
	{
	}

	public UnauthorizedAccessException(string errorCode, object parameters)
		: base(Error.Create(
			errorCode,
			"Unauthorized access attempt.",
			parameters))
	{
	}

	public UnauthorizedAccessException(Error error)
		: base(error)
	{
	}
}