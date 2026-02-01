namespace CoreBackend.Contracts.Auth.Requests;

public class DisableTwoFactorRequest
{
	public string Password { get; set; } = null!;
	public string? Code { get; set; } // 2FA aktifse kod gerekli
}