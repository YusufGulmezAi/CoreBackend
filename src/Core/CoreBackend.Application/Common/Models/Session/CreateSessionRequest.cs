namespace CoreBackend.Application.Common.Models.Session;

/// <summary>
/// Yeni oturum oluþturma isteði.
/// </summary>
public sealed record CreateSessionRequest
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
	/// Cihaz bilgisi.
	/// </summary>
	public required DeviceInfo Device { get; init; }

	/// <summary>
	/// Oturum süresi.
	/// </summary>
	public required TimeSpan SessionDuration { get; init; }

	/// <summary>
	/// IP deðiþikliðine izin var mý?
	/// </summary>
	public bool AllowIpChange { get; init; }

	/// <summary>
	/// Browser deðiþikliðine izin var mý?
	/// </summary>
	public bool AllowBrowserChange { get; init; }
}