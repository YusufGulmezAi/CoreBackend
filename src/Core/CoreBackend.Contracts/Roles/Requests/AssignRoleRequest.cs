namespace CoreBackend.Contracts.Roles.Requests;

public class AssignRoleRequest
{
	public Guid UserId { get; set; }
	public Guid RoleId { get; set; }
	public Guid? CompanyId { get; set; } // Company rolü için
}