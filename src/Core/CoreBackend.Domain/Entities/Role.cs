using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// Role entity'si.
/// Kullanıcılara atanabilecek rolleri tanımlar.
/// </summary>
public class Role : TenantAuditableEntity<Guid>
{
	/// <summary>
	/// Rol adı.
	/// </summary>
	public string Name { get; private set; } = null!;

	/// <summary>
	/// Rol kodu (benzersiz, programatik erişim için).
	/// </summary>
	public string Code { get; private set; } = null!;


	/// <summary>
	/// Rol bazlı session zaman aşımı süresi (dakika).
	/// null ise tenant veya global ayar kullanılır.
	/// </summary>
	public int? SessionTimeoutMinutes { get; private set; }


	/// <summary>
	/// Rol açıklaması.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Rol seviyesi (System, Tenant, Company).
	/// </summary>
	public RoleLevel Level { get; private set; }

	/// <summary>
	/// Sistem rolü mü? Sistem rolleri silinemez/değiştirilemez.
	/// </summary>
	public bool IsSystemRole { get; private set; }

	/// <summary>
	/// Aktif mi?
	/// </summary>
	public bool IsActive { get; private set; }

	/// <summary>
	/// Kullanıcı-Rol ilişkileri.
	/// </summary>
	public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

	/// <summary>
	/// Rol-Permission ilişkileri.
	/// </summary>
	public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

	// EF Core için private constructor
	private Role() : base() { }

	private Role(
		Guid id,
		Guid tenantId,
		string name,
		string code,
		string? description,
		RoleLevel level,
		bool isSystemRole,
		int? sessionTimeoutMinutes) : base(id, tenantId)
	{
		Name = name;
		Code = code;
		Description = description;
		Level = level;
		IsSystemRole = isSystemRole;
		IsActive = true;
		SessionTimeoutMinutes = sessionTimeoutMinutes;
	}

	/// <summary>
	/// Yeni rol oluşturur.
	/// </summary>
	public static Role Create(
		Guid tenantId,
		string name,
		string code,
		RoleLevel level,
		string? description = null,
		bool isSystemRole = false,
		int? sessionTimeoutMinutes = null)
	{
		return new Role(
			Guid.NewGuid(),
			tenantId,
			name,
			code,
			description,
			level,
			isSystemRole,
			sessionTimeoutMinutes);
	}

	/// <summary>
	/// Rol bilgilerini günceller.
	/// </summary>
	public void Update(string name, string? description)
	{
		if (IsSystemRole)
			return;

		Name = name;
		Description = description;
	}

	/// <summary>
	/// Rolü aktif eder.
	/// </summary>
	public void Activate()
	{
		IsActive = true;
	}

	/// <summary>
	/// Rolü pasif eder.
	/// </summary>
	public void Deactivate()
	{
		if (IsSystemRole)
			return;

		IsActive = false;
	}

	/// <summary>
	/// Session timeout süresini günceller.
	/// </summary>
	public void UpdateSessionTimeout(int? sessionTimeoutMinutes)
	{
		SessionTimeoutMinutes = sessionTimeoutMinutes;
	}

}