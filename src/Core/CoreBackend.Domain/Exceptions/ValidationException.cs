using CoreBackend.Domain.Errors;

namespace CoreBackend.Domain.Exceptions;

/// <summary>
/// Validasyon hatalarında fırlatılır.
/// HTTP 400 Bad Request ile eşleşir.
/// Birden fazla hata mesajı taşıyabilir.
/// </summary>
public class ValidationException : DomainException
{
	/// <summary>
	/// Property bazlı validasyon hataları.
	/// Key: Property adı, Value: Hata kodları listesi
	/// </summary>
	public IReadOnlyDictionary<string, string[]> Errors { get; }

	public ValidationException(IReadOnlyDictionary<string, string[]> errors)
		: base(Error.Create(
			ErrorCodes.General.ValidationError,
			"One or more validation errors occurred.",
			new { errorCount = errors.Count }))
	{
		Errors = errors;
	}

	public ValidationException(string propertyName, string errorCode)
		: base(Error.Create(
			ErrorCodes.General.ValidationError,
			"Validation error occurred.",
			new { propertyName }))
	{
		Errors = new Dictionary<string, string[]>
		{
			{ propertyName, new[] { errorCode } }
		};
	}

	public ValidationException(string errorCode)
		: base(Error.Create(
			errorCode,
			"Validation error occurred."))
	{
		Errors = new Dictionary<string, string[]>();
	}
}