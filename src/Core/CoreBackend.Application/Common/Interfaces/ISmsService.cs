namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// SMS servis interface.
/// </summary>
public interface ISmsService
{
	/// <summary>
	/// SMS gönderir.
	/// </summary>
	Task<SmsResult> SendAsync(
		string phoneNumber,
		string message,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 2FA kodu gönderir.
	/// </summary>
	Task<SmsResult> SendTwoFactorCodeAsync(
		string phoneNumber,
		string code,
		int expirationMinutes,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// SMS gönderim sonucu.
/// </summary>
public class SmsResult
{
	public bool Success { get; set; }
	public string? MessageId { get; set; }
	public string? ErrorCode { get; set; }
	public string? ErrorMessage { get; set; }

	public static SmsResult Succeeded(string? messageId = null) => new()
	{
		Success = true,
		MessageId = messageId
	};

	public static SmsResult Failed(string errorCode, string errorMessage) => new()
	{
		Success = false,
		ErrorCode = errorCode,
		ErrorMessage = errorMessage
	};
}