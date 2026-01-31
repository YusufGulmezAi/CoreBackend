namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Current user servis interface.
/// JWT Token'dan gelen temel kullanıcı bilgilerine erişim sağlar.
/// Detaylı bilgiler (roller, izinler) cache'te tutulur.
/// </summary>
public interface ICurrentUserService
{
	/// <summary>
	/// Mevcut kullanıcı Id (Token'dan).
	/// </summary>
	Guid? UserId { get; }

	/// <summary>
	/// Mevcut Tenant Id (Token'dan).
	/// </summary>
	Guid? TenantId { get; }

	/// <summary>
	/// Mevcut Session Id (Token'dan, cache key olarak kullanılır).
	/// </summary>
	string? SessionId { get; }

	/// <summary>
	/// Kullanıcı oturum açmış mı?
	/// </summary>
	bool IsAuthenticated { get; }
}