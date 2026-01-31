using CoreBackend.Domain.Errors;

namespace CoreBackend.Domain.Exceptions;

/// <summary>
/// Çakışma durumlarında fırlatılır.
/// Duplicate kayıt veya eş zamanlılık hatası.
/// HTTP 409 Conflict ile eşleşir.
/// </summary>
public class ConflictException : DomainException
{
	public ConflictException()
		: base(Error.Create(
			ErrorCodes.General.Conflict,
			"A conflict occurred."))
	{
	}

	public ConflictException(string errorCode)
		: base(Error.Create(
			errorCode,
			"A conflict occurred.",
			new { errorCode }))
	{
	}

	public ConflictException(Error error)
		: base(error)
	{
	}

	private ConflictException(string errorCode, object parameters)
		: base(Error.Create(
			errorCode,
			"A conflict occurred.",
			parameters))
	{
	}

	/// <summary>
	/// Parametreli conflict exception oluşturur.
	/// </summary>
	public static ConflictException WithParameters(string errorCode, object parameters)
		=> new(errorCode, parameters);

	/// <summary>
	/// Entity bazlı conflict exception oluşturur.
	/// </summary>
	public static ConflictException ForEntity(string entityName, object id)
		=> new(ErrorCodes.General.Conflict, new { entityName, id });
}