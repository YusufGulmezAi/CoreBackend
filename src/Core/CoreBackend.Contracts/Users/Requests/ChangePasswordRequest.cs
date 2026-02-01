namespace CoreBackend.Contracts.Users.Requests;

public class ChangePasswordRequest
{
	public string CurrentPassword { get; set; } = null!;
	public string NewPassword { get; set; } = null!;
	public string ConfirmNewPassword { get; set; } = null!;
}