namespace CoreBackend.Contracts.Roles.Requests;

public class CreateRoleRequest
{
	public string Name { get; set; } = null!;
	public string Code { get; set; } = null!;
	public string? Description { get; set; }
	public string Level { get; set; } = null!; // Tenant, Company
	public List<Guid>? PermissionIds { get; set; }
}