namespace CoreBackend.Contracts.Auth.Responses;

public class TwoFactorRequiredResponse
{
	public bool TwoFactorRequired { get; set; } = true;
	public string SessionToken { get; set; } = null!; // Geçici token (2FA doğrulaması için)
	public string Method { get; set; } = null!; // Hangi metod kullanılacak
	public bool CanUseRecoveryCode { get; set; }
	public string? Message { get; set; } // "Email'inize kod gönderildi" vb.
}