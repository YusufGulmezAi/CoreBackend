using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// UserCompanyRole entity'si.
/// Company seviyesinde kullanıcı-rol ilişkisini tanımlar.
/// Aynı kullanıcı farklı şirketlerde farklı rollere sahip olabilir.
/// </summary>
public class UserCompanyRole : CompanyAuditableEntity<Guid>
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
	private UserCompanyRole() : base() { }

	private UserCompanyRole(
		Guid id,
		Guid tenantId,
		Guid companyId,
		Guid userId,
		Guid roleId,
		DateTime? expiresAt) : base(id, tenantId, companyId)
	{
		UserId = userId;
		RoleId = roleId;
		AssignedAt = DateTime.UtcNow;
		ExpiresAt = expiresAt;
		IsActive = true;
	}

	/// <summary>
	/// Yeni kullanıcı-şirket-rol ataması oluşturur.
	/// </summary>
	public static UserCompanyRole Create(
		Guid tenantId,
		Guid companyId,
		Guid userId,
		Guid roleId,
		DateTime? expiresAt = null)
	{
		return new UserCompanyRole(
			Guid.NewGuid(),
			tenantId,
			companyId,
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