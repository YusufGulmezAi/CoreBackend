namespace CoreBackend.Contracts.Auth.Requests;

public class EnableTwoFactorRequest
{
	public string Method { get; set; } = null!; // Email, Totp, Sms
}