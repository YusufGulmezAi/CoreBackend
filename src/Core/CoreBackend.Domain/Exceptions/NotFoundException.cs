using CoreBackend.Domain.Errors;

namespace CoreBackend.Domain.Exceptions;

/// <summary>
/// Aranan kayıt bulunamadığında fırlatılır.
/// HTTP 404 Not Found ile eşleşir.
/// </summary>
public class NotFoundException : DomainException
{
	public NotFoundException(Error error)
		: base(error)
	{
	}

	public NotFoundException(string message)
		: base(ErrorCodes.General.NotFound, message)
	{
	}

	public NotFoundException(string entityName, object id)
		: base(
			ErrorCodes.General.NotFound,
			$"{entityName} bulunamadı.",
			new { entityName, id })
	{
	}
}