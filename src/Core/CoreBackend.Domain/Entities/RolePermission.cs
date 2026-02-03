using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// RolePermission entity'si.
/// Rol ve izin arasındaki ilişkiyi tanımlar.
/// </summary>
public class RolePermission : TenantAuditableEntity<Guid>
{
	/// <summary>
	/// Rol Id.
	/// </summary>
	public Guid RoleId { get; private set; }

	/// <summary>
	/// Rol.
	/// </summary>
	public Role Role { get; private set; } = null!;
	/// <summary>
	/// İzin Id.
	/// </summary>
	public Guid PermissionId { get; private set; }

	/// <summary>
	/// İzinler
	/// </summary>
	public Permission Permission { get; private set; } = null!;
	// EF Core için private constructor
	private RolePermission() : base() { }

	private RolePermission(
		Guid id,
		Guid tenantId,
		Guid roleId,
		Guid permissionId) : base(id, tenantId)
	{
		RoleId = roleId;
		PermissionId = permissionId;
	}

	/// <summary>
	/// Yeni rol-izin ataması oluşturur.
	/// </summary>
	public static RolePermission Create(
		Guid tenantId,
		Guid roleId,
		Guid permissionId)
	{
		return new RolePermission(
			Guid.NewGuid(),
			tenantId,
			roleId,
			permissionId);
	}
}