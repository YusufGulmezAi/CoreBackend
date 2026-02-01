namespace CoreBackend.Contracts.Auth.Requests;

public class VerifyTwoFactorRequest
{
	public string Code { get; set; } = null!;
	public string? SessionToken { get; set; } // Login sırasında dönen geçici token
}