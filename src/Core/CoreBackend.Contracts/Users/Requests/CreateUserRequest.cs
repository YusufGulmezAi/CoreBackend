namespace CoreBackend.Contracts.Users.Requests;

public class CreateUserRequest
{
	public string Username { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string Password { get; set; } = null!;
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public string? Phone { get; set; }
	public List<Guid>? RoleIds { get; set; }
}