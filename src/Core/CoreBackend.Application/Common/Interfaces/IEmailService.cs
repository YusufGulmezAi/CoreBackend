namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Email servis interface.
/// </summary>
public interface IEmailService
{
	/// <summary>
	/// Email gönderir.
	/// </summary>
	Task<bool> SendAsync(
		string to,
		string subject,
		string body,
		bool isHtml = true,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 2FA kodu gönderir.
	/// </summary>
	Task<bool> SendTwoFactorCodeAsync(
		string to,
		string code,
		int expirationMinutes,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Şifre sıfırlama emaili gönderir.
	/// </summary>
	Task<bool> SendPasswordResetAsync(
		string to,
		string resetLink,
		CancellationToken cancellationToken = default);
}