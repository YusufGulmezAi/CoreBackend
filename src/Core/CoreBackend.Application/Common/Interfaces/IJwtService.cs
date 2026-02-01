using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// JWT token servisi interface.
/// Token oluşturma ve doğrulama işlemleri.
/// </summary>
public interface IJwtService
{
	/// <summary>
	/// Access token oluşturur.
	/// </summary>
	string GenerateAccessToken(JwtUserData userData);

	/// <summary>
	/// Refresh token oluşturur.
	/// </summary>
	string GenerateRefreshToken();

	/// <summary>
	/// Token'ı doğrular ve claim'leri döner.
	/// </summary>
	JwtUserData? ValidateToken(string token);

	/// <summary>
	/// Token'dan user data çıkarır (süre dolmuş olsa bile).
	/// </summary>
	JwtUserData? GetUserDataFromExpiredToken(string token);

	/// <summary>
	/// Access token bitiş süresini döner.
	/// </summary>
	DateTime GetAccessTokenExpiration();

	/// <summary>
	/// Refresh token bitiş süresini döner.
	/// </summary>
	DateTime GetRefreshTokenExpiration();
}

/// <summary>
/// JWT token içinde taşınacak minimal kullanıcı verisi.
/// </summary>
public class JwtUserData
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
	/// Session Id (Cache key).
	/// </summary>
	public string SessionId { get; set; } = null!;

	/// <summary>
	/// Email adresi.
	/// </summary>
	public string Email { get; set; } = null!;
}