namespace CoreBackend.Contracts.Users.Responses;

public class UserResponse
{
	public Guid Id { get; set; }
	public Guid TenantId { get; set; }
	public string Username { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string FirstName { get; set; } = null!;
	public string LastName { get; set; } = null!;
	public string FullName { get; set; } = null!;
	public string? Phone { get; set; }
	public string Status { get; set; } = null!;
	public bool EmailConfirmed { get; set; }
	public DateTime? LastLoginAt { get; set; }
	public DateTime CreatedAt { get; set; }
	public List<string> Roles { get; set; } = new();
}