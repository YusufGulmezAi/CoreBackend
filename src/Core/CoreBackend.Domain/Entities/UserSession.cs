using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// UserSession entity'si.
/// Kullanıcı oturumlarını takip eder.
/// Cache'teki session verisinin kalıcı kaydı.
/// </summary>
public class UserSession : TenantAuditableEntity<Guid>
{
	/// <summary>
	/// Kullanıcı Id.
	/// </summary>
	public Guid UserId { get; private set; }

	/// <summary>
	/// Session Id (Cache key, Token'da taşınır).
	/// </summary>
	public string SessionId { get; private set; } = null!;

	/// <summary>
	/// Refresh Token.
	/// </summary>
	public string? RefreshToken { get; private set; }

	/// <summary>
	/// Refresh Token bitiş tarihi.
	/// </summary>
	public DateTime? RefreshTokenExpiresAt { get; private set; }

	/// <summary>
	/// Oturum açılan IP adresi.
	/// </summary>
	public string IpAddress { get; private set; } = null!;

	/// <summary>
	/// User Agent (tarayıcı/cihaz bilgisi).
	/// </summary>
	public string UserAgent { get; private set; } = null!;

	/// <summary>
	/// Coğrafi konum (JSON formatında).
	/// </summary>
	public string? GeoLocation { get; private set; }

	/// <summary>
	/// Tarayıcı adı.
	/// </summary>
	public string? BrowserName { get; private set; }

	/// <summary>
	/// İşletim sistemi.
	/// </summary>
	public string? OperatingSystem { get; private set; }

	/// <summary>
	/// Cihaz tipi (Desktop, Mobile, Tablet).
	/// </summary>
	public string? DeviceType { get; private set; }

	/// <summary>
	/// Session oluşturulma tarihi.
	/// </summary>
	public DateTime StartedAt { get; private set; }

	/// <summary>
	/// Session bitiş tarihi.
	/// </summary>
	public DateTime ExpiresAt { get; private set; }

	/// <summary>
	/// Son aktivite tarihi.
	/// </summary>
	public DateTime LastActivityAt { get; private set; }

	/// <summary>
	/// Session sonlandırıldı mı?
	/// </summary>
	public bool IsRevoked { get; private set; }

	/// <summary>
	/// Sonlandırılma tarihi.
	/// </summary>
	public DateTime? RevokedAt { get; private set; }

	/// <summary>
	/// Sonlandırılma nedeni.
	/// </summary>
	public string? RevokedReason { get; private set; }

	/// <summary>
	/// Sonlandıran kullanıcı Id (Admin müdahalesi için).
	/// </summary>
	public Guid? RevokedBy { get; private set; }

	/// <summary>
	/// IP değişikliğine izin var mı?
	/// </summary>
	public bool AllowIpChange { get; private set; }

	/// <summary>
	/// Browser değişikliğine izin var mı?
	/// </summary>
	public bool AllowUserAgentChange { get; private set; }

	// EF Core için private constructor
	private UserSession() : base() { }

	private UserSession(
		Guid id,
		Guid tenantId,
		Guid userId,
		string sessionId,
		string ipAddress,
		string userAgent,
		DateTime expiresAt,
		bool allowIpChange,
		bool allowUserAgentChange) : base(id, tenantId)
	{
		UserId = userId;
		SessionId = sessionId;
		IpAddress = ipAddress;
		UserAgent = userAgent;
		StartedAt = DateTime.UtcNow;
		ExpiresAt = expiresAt;
		LastActivityAt = DateTime.UtcNow;
		IsRevoked = false;
		AllowIpChange = allowIpChange;
		AllowUserAgentChange = allowUserAgentChange;
	}

	/// <summary>
	/// Yeni session oluşturur.
	/// </summary>
	public static UserSession Create(
		Guid tenantId,
		Guid userId,
		string sessionId,
		string ipAddress,
		string userAgent,
		DateTime expiresAt,
		bool allowIpChange = false,
		bool allowUserAgentChange = false)
	{
		return new UserSession(
			Guid.NewGuid(),
			tenantId,
			userId,
			sessionId,
			ipAddress,
			userAgent,
			expiresAt,
			allowIpChange,
			allowUserAgentChange);
	}

	/// <summary>
	/// Cihaz bilgilerini ayarlar.
	/// </summary>
	public void SetDeviceInfo(
		string? geoLocation,
		string? browserName,
		string? operatingSystem,
		string? deviceType)
	{
		GeoLocation = geoLocation;
		BrowserName = browserName;
		OperatingSystem = operatingSystem;
		DeviceType = deviceType;
	}

	/// <summary>
	/// Refresh token ayarlar.
	/// </summary>
	public void SetRefreshToken(string refreshToken, DateTime expiresAt)
	{
		RefreshToken = refreshToken;
		RefreshTokenExpiresAt = expiresAt;
	}

	/// <summary>
	/// Son aktivite tarihini günceller.
	/// </summary>
	public void UpdateLastActivity()
	{
		LastActivityAt = DateTime.UtcNow;
	}

	/// <summary>
	/// Session'ı sonlandırır.
	/// </summary>
	public void Revoke(string reason, Guid? revokedBy = null)
	{
		IsRevoked = true;
		RevokedAt = DateTime.UtcNow;
		RevokedReason = reason;
		RevokedBy = revokedBy;
	}

	/// <summary>
	/// Session'ın aktif olup olmadığını kontrol eder.
	/// </summary>
	public bool IsActive()
	{
		if (IsRevoked)
			return false;

		if (DateTime.UtcNow > ExpiresAt)
			return false;

		return true;
	}
}