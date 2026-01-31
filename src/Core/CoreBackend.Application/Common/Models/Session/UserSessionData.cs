namespace CoreBackend.Application.Common.Models.Session;

/// <summary>
/// Cache'te tutulan oturum verisi.
/// </summary>
public sealed record UserSessionData
{
	/// <summary>
	/// Kullanýcý Id.
	/// </summary>
	public required Guid UserId { get; init; }

	/// <summary>
	/// Tenant Id.
	/// </summary>
	public required Guid TenantId { get; init; }

	/// <summary>
	/// Kullanýcý email.
	/// </summary>
	public required string Email { get; init; }

	/// <summary>
	/// Mevcut seçili Company Id.
	/// </summary>
	public Guid? CompanyId { get; init; }

	/// <summary>
	/// Tenant seviyesi roller.
	/// </summary>
	public required IReadOnlyList<string> TenantRoles { get; init; }

	/// <summary>
	/// Company seviyesi roller.
	/// </summary>
	public required IReadOnlyList<string> CompanyRoles { get; init; }

	/// <summary>
	/// Tüm izinler.
	/// </summary>
	public required IReadOnlyList<string> Permissions { get; init; }

	/// <summary>
	/// Oturum oluþturulduðundaki cihaz bilgisi.
	/// </summary>
	public required DeviceInfo Device { get; init; }

	/// <summary>
	/// Oturum oluþturulma zamaný.
	/// </summary>
	public required DateTime CreatedAt { get; init; }

	/// <summary>
	/// Son aktivite zamaný.
	/// </summary>
	public DateTime LastActivityAt { get; init; }

	/// <summary>
	/// Oturum son kullanma tarihi.
	/// </summary>
	public required DateTime ExpiresAt { get; init; }

	/// <summary>
	/// IP deðiþikliðine izin var mý?
	/// </summary>
	public bool AllowIpChange { get; init; }

	/// <summary>
	/// Browser deðiþikliðine izin var mý?
	/// </summary>
	public bool AllowBrowserChange { get; init; }

	/// <summary>
	/// Kullanýcýnýn belirli bir izne sahip olup olmadýðýný kontrol eder.
	/// </summary>
	public bool HasPermission(string permission) =>
		Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Kullanýcýnýn belirli bir role sahip olup olmadýðýný kontrol eder.
	/// </summary>
	public bool HasRole(string role) =>
		TenantRoles.Contains(role, StringComparer.OrdinalIgnoreCase) ||
		CompanyRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
}