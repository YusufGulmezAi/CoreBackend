namespace CoreBackend.Contracts.Auth.Responses;

public class TwoFactorSetupResponse
{
	public string Method { get; set; } = null!;
	public string? SecretKey { get; set; } // TOTP için
	public string? QrCodeUri { get; set; } // TOTP için
	public string? Message { get; set; } // Email/SMS için "Kod gönderildi" mesajı
	public List<string>? RecoveryCodes { get; set; } // Setup tamamlandığında
}