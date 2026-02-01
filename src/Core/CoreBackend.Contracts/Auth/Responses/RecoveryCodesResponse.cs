namespace CoreBackend.Contracts.Auth.Responses;

public class RecoveryCodesResponse
{
	public List<string> RecoveryCodes { get; set; } = new();
	public string Message { get; set; } = null!;
}