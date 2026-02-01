namespace CoreBackend.Contracts.Users.Requests;

public class UpdateUserRequest
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Phone { get; set; }
}