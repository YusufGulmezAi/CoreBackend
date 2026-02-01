namespace CoreBackend.Contracts.Roles.Responses;

public class RoleResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
	public string Code { get; set; } = null!;
	public string? Description { get; set; }
	public string Level { get; set; } = null!;
	public bool IsSystemRole { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
	public List<PermissionResponse> Permissions { get; set; } = new();
}

public class PermissionResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
	public string Code { get; set; } = null!;
	public string Group { get; set; } = null!;
	public string? Description { get; set; }
}