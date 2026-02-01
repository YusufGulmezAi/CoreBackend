namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// User session servis interface.
/// </summary>
public interface IUserSessionService
{
	/// <summary>
	/// Session oluþturur.
	/// </summary>
	Task<string> CreateSessionAsync(
		UserSessionData sessionData,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Session getirir.
	/// </summary>
	Task<UserSessionData?> GetSessionAsync(
		string sessionId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Session günceller.
	/// </summary>
	Task UpdateSessionAsync(
		string sessionId,
		UserSessionData sessionData,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Session'ý iptal eder.
	/// </summary>
	Task RevokeSessionAsync(
		string sessionId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Kullanýcýnýn tüm session'larýný iptal eder.
	/// </summary>
	Task RevokeAllUserSessionsAsync(
		Guid userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Kullanýcýnýn aktif session'larýný getirir.
	/// </summary>
	Task<IReadOnlyList<UserSessionData>> GetUserActiveSessionsAsync(
		Guid userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Session'ýn geçerliliðini kontrol eder.
	/// </summary>
	Task<bool> ValidateSessionAsync(
		string sessionId,
		string? ipAddress = null,
		string? userAgent = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Session aktivitesini günceller.
	/// </summary>
	Task RefreshSessionActivityAsync(
		string sessionId,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache'te saklanan session verisi.
/// </summary>
public class UserSessionData
{
	/// <summary>
	/// Kullanýcý Id.
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Tenant Id.
	/// </summary>
	public Guid TenantId { get; set; }

	/// <summary>
	/// Seçili Company Id (opsiyonel).
	/// </summary>
	public Guid? CompanyId { get; set; }

	/// <summary>
	/// Kullanýcý email.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Kullanýcý tam adý.
	/// </summary>
	public string FullName { get; set; } = null!;

	/// <summary>
	/// Tenant seviyesindeki roller.
	/// </summary>
	public List<string> TenantRoles { get; set; } = new();

	/// <summary>
	/// Company seviyesindeki roller.
	/// </summary>
	public List<string> CompanyRoles { get; set; } = new();

	/// <summary>
	/// Tüm izinler (Tenant + Company rolleri birleþik).
	/// </summary>
	public List<string> Permissions { get; set; } = new();

	/// <summary>
	/// Oturum açýlan IP adresi.
	/// </summary>
	public string IpAddress { get; set; } = null!;

	/// <summary>
	/// Kullanýcý tarayýcý/cihaz bilgisi.
	/// </summary>
	public string UserAgent { get; set; } = null!;

	/// <summary>
	/// Coðrafi konum bilgisi.
	/// </summary>
	public string? GeoLocation { get; set; }

	/// <summary>
	/// Session oluþturulma tarihi.
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Son aktivite tarihi.
	/// </summary>
	public DateTime LastActivityAt { get; set; }

	/// <summary>
	/// Session bitiþ tarihi.
	/// </summary>
	public DateTime ExpiresAt { get; set; }

	/// <summary>
	/// IP deðiþikliðine izin var mý?
	/// </summary>
	public bool AllowIpChange { get; set; }

	/// <summary>
	/// Browser deðiþikliðine izin var mý?
	/// </summary>
	public bool AllowUserAgentChange { get; set; }
}

/// <summary>
/// Session doðrulama sonucu.
/// </summary>
public class SessionValidationResult
{
	/// <summary>
	/// Session geçerli mi?
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// Geçersizse hata kodu.
	/// </summary>
	public string? ErrorCode { get; set; }

	/// <summary>
	/// Geçersizse hata mesajý.
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Session verisi (geçerliyse).
	/// </summary>
	public UserSessionData? SessionData { get; set; }

	public static SessionValidationResult Success(UserSessionData sessionData)
		=> new() { IsValid = true, SessionData = sessionData };

	public static SessionValidationResult Failed(string errorCode, string errorMessage)
		=> new() { IsValid = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}