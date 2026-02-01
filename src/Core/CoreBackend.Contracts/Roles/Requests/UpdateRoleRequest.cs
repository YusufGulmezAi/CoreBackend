namespace CoreBackend.Contracts.Roles.Requests;

public class UpdateRoleRequest
{
	public string Name { get; set; } = null!;
	public string? Description { get; set; }
	public List<Guid>? PermissionIds { get; set; }
}