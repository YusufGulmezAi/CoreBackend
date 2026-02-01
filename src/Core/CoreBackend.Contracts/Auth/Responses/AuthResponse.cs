namespace CoreBackend.Contracts.Auth.Responses;

/// <summary>
/// Authentication response.
/// Login ve Register sonrası döner.
/// </summary>
public class AuthResponse
{
	/// <summary>
	/// Kullanıcı Id.
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Tenant Id.
	/// </summary>
	public Guid TenantId { get; set; }

	/// <summary>
	/// Email adresi.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Kullanıcı tam adı.
	/// </summary>
	public string FullName { get; set; } = null!;

	/// <summary>
	/// Access token (JWT).
	/// </summary>
	public string AccessToken { get; set; } = null!;

	/// <summary>
	/// Refresh token.
	/// </summary>
	public string RefreshToken { get; set; } = null!;

	/// <summary>
	/// Access token bitiş tarihi.
	/// </summary>
	public DateTime AccessTokenExpiresAt { get; set; }

	/// <summary>
	/// Refresh token bitiş tarihi.
	/// </summary>
	public DateTime RefreshTokenExpiresAt { get; set; }

	/// <summary>
	/// Kullanıcının Tenant seviyesindeki rolleri.
	/// </summary>
	public List<string> TenantRoles { get; set; } = new();

	/// <summary>
	/// Kullanıcının erişebileceği şirketler.
	/// </summary>
	public List<UserCompanyInfo> Companies { get; set; } = new();
}

/// <summary>
/// Kullanıcının erişebileceği şirket bilgisi.
/// </summary>
public class UserCompanyInfo
{
	/// <summary>
	/// Şirket Id.
	/// </summary>
	public Guid CompanyId { get; set; }

	/// <summary>
	/// Şirket adı.
	/// </summary>
	public string CompanyName { get; set; } = null!;

	/// <summary>
	/// Şirket kodu.
	/// </summary>
	public string CompanyCode { get; set; } = null!;

	/// <summary>
	/// Bu şirketteki roller.
	/// </summary>
	public List<string> Roles { get; set; } = new();
}