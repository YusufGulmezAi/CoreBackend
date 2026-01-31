namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// JWT Token'dan gelen temel kullanıcı bilgileri.
/// Sadece token'da bulunan minimal bilgileri içerir.
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
	/// Oturum Id - Cache key olarak kullanılır (Token'dan).
	/// </summary>
	string? SessionId { get; }

	/// <summary>
	/// Token geçerli mi?
	/// </summary>
	bool IsAuthenticated { get; }
}