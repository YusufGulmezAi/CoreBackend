using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// UserRole entity'si.
/// Tenant seviyesinde kullanıcı-rol ilişkisini tanımlar.
/// </summary>
public class UserRole : TenantAuditableEntity<Guid>
{
	/// <summary>
	/// Kullanıcı Id.
	/// </summary>
	public Guid UserId { get; private set; }

	/// <summary>
	/// Rol Id.
	/// </summary>
	public Guid RoleId { get; private set; }

	/// <summary>
	/// Atama başlangıç tarihi.
	/// </summary>
	public DateTime AssignedAt { get; private set; }

	/// <summary>
	/// Atama bitiş tarihi (opsiyonel, geçici atamalar için).
	/// </summary>
	public DateTime? ExpiresAt { get; private set; }

	/// <summary>
	/// Aktif mi?
	/// </summary>
	public bool IsActive { get; private set; }

	// EF Core için private constructor
	private UserRole() : base() { }

	private UserRole(
		Guid id,
		Guid tenantId,
		Guid userId,
		Guid roleId,
		DateTime? expiresAt) : base(id, tenantId)
	{
		UserId = userId;
		RoleId = roleId;
		AssignedAt = DateTime.UtcNow;
		ExpiresAt = expiresAt;
		IsActive = true;
	}

	/// <summary>
	/// Yeni kullanıcı-rol ataması oluşturur.
	/// </summary>
	public static UserRole Create(
		Guid tenantId,
		Guid userId,
		Guid roleId,
		DateTime? expiresAt = null)
	{
		return new UserRole(
			Guid.NewGuid(),
			tenantId,
			userId,
			roleId,
			expiresAt);
	}

	/// <summary>
	/// Atamayı aktif eder.
	/// </summary>
	public void Activate()
	{
		IsActive = true;
	}

	/// <summary>
	/// Atamayı pasif eder.
	/// </summary>
	public void Deactivate()
	{
		IsActive = false;
	}

	/// <summary>
	/// Bitiş tarihini günceller.
	/// </summary>
	public void UpdateExpiration(DateTime? expiresAt)
	{
		ExpiresAt = expiresAt;
	}

	/// <summary>
	/// Atamanın geçerli olup olmadığını kontrol eder.
	/// </summary>
	public bool IsValid()
	{
		if (!IsActive)
			return false;

		if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow)
			return false;

		return true;
	}
}