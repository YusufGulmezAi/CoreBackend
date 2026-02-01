namespace CoreBackend.Contracts.Auth.Requests;

public class UseRecoveryCodeRequest
{
	public string RecoveryCode { get; set; } = null!;
	public string? SessionToken { get; set; }
}