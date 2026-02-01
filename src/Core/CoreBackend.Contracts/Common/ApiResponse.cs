namespace CoreBackend.Contracts.Common;

/// <summary>
/// Standart API response wrapper.
/// Tüm API yanıtları bu formatta döner.
/// </summary>
/// <typeparam name="T">Data tipi</typeparam>
public class ApiResponse<T>
{
	/// <summary>
	/// İşlem başarılı mı?
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Response data.
	/// </summary>
	public T? Data { get; set; }

	/// <summary>
	/// Hata mesajı (başarısızsa).
	/// </summary>
	public string? Message { get; set; }

	/// <summary>
	/// Hata kodu (başarısızsa).
	/// </summary>
	public string? ErrorCode { get; set; }

	/// <summary>
	/// Validation hataları.
	/// </summary>
	public Dictionary<string, string[]>? Errors { get; set; }

	/// <summary>
	/// Başarılı response oluşturur.
	/// </summary>
	public static ApiResponse<T> SuccessResponse(T data, string? message = null)
	{
		return new ApiResponse<T>
		{
			Success = true,
			Data = data,
			Message = message
		};
	}

	/// <summary>
	/// Başarısız response oluşturur.
	/// </summary>
	public static ApiResponse<T> FailureResponse(string message, string? errorCode = null)
	{
		return new ApiResponse<T>
		{
			Success = false,
			Message = message,
			ErrorCode = errorCode
		};
	}

	/// <summary>
	/// Validation hatası response'u oluşturur.
	/// </summary>
	public static ApiResponse<T> ValidationFailure(Dictionary<string, string[]> errors)
	{
		return new ApiResponse<T>
		{
			Success = false,
			Message = "Validation failed",
			ErrorCode = "VALIDATION_ERROR",
			Errors = errors
		};
	}
}

/// <summary>
/// Data içermeyen API response.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
	/// <summary>
	/// Başarılı response oluşturur.
	/// </summary>
	public static ApiResponse SuccessResponse(string? message = null)
	{
		return new ApiResponse
		{
			Success = true,
			Message = message
		};
	}

	/// <summary>
	/// Başarısız response oluşturur.
	/// </summary>
	public new static ApiResponse FailureResponse(string message, string? errorCode = null)
	{
		return new ApiResponse
		{
			Success = false,
			Message = message,
			ErrorCode = errorCode
		};
	}
}