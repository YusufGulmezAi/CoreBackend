namespace CoreBackend.Contracts.Auth.Requests;

public class VerifyTwoFactorSetupRequest
{
	public string Code { get; set; } = null!;
	public string Method { get; set; } = null!;
	public string? SecretKey { get; set; } // TOTP için
}