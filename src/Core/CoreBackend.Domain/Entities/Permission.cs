using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// Permission entity'si.
/// Sistemdeki izinleri tanımlar.
/// Örnek: Users.Create, Reports.View, Companies.Delete
/// </summary>
public class Permission : AuditableEntity<Guid>
{
	/// <summary>
	/// İzin adı.
	/// </summary>
	public string Name { get; private set; } = null!;

	/// <summary>
	/// İzin kodu (benzersiz, programatik erişim için).
	/// Format: Resource.Action (örn: Users.Create)
	/// </summary>
	public string Code { get; private set; } = null!;

	/// <summary>
	/// İzin açıklaması.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// İzin grubu (menü/modül bazlı gruplama için).
	/// Örnek: UserManagement, Reports, Settings
	/// </summary>
	public string Group { get; private set; } = null!;

	/// <summary>
	/// Aktif mi?
	/// </summary>
	public bool IsActive { get; private set; }

	/// <summary>
	/// Rol-Permission ilişkileri.
	/// </summary>
	public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

	// EF Core için private constructor
	private Permission() : base() { }

	private Permission(
		Guid id,
		string name,
		string code,
		string group,
		string? description) : base(id)
	{
		Name = name;
		Code = code;
		Group = group;
		Description = description;
		IsActive = true;
	}

	/// <summary>
	/// Yeni izin oluşturur.
	/// </summary>
	public static Permission Create(
		string name,
		string code,
		string group,
		string? description = null)
	{
		return new Permission(
			Guid.NewGuid(),
			name,
			code,
			group,
			description);
	}

	/// <summary>
	/// İzin bilgilerini günceller.
	/// </summary>
	public void Update(string name, string? description)
	{
		Name = name;
		Description = description;
	}

	/// <summary>
	/// İzni aktif eder.
	/// </summary>
	public void Activate()
	{
		IsActive = true;
	}

	/// <summary>
	/// İzni pasif eder.
	/// </summary>
	public void Deactivate()
	{
		IsActive = false;
	}
}