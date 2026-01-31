namespace CoreBackend.Application.Common.Models.Session;

/// <summary>
/// Oturum güvenlik doðrulama sonucu.
/// </summary>
public sealed record SessionValidationResult
{
	/// <summary>
	/// Doðrulama baþarýlý mý?
	/// </summary>
	public bool IsValid { get; init; }

	/// <summary>
	/// Geçersizlik nedeni.
	/// </summary>
	public SessionInvalidReason? InvalidReason { get; init; }

	/// <summary>
	/// Hata mesajý.
	/// </summary>
	public string? Message { get; init; }

	/// <summary>
	/// Baþarýlý sonuç oluþturur.
	/// </summary>
	public static SessionValidationResult Success() =>
		new() { IsValid = true };

	/// <summary>
	/// Baþarýsýz sonuç oluþturur.
	/// </summary>
	public static SessionValidationResult Failure(SessionInvalidReason reason, string message) =>
		new() { IsValid = false, InvalidReason = reason, Message = message };
}

/// <summary>
/// Oturum geçersizlik nedenleri.
/// </summary>
public enum SessionInvalidReason
{
	/// <summary>
	/// Oturum bulunamadý (cache'ten silinmiþ).
	/// </summary>
	SessionNotFound = 1,

	/// <summary>
	/// Oturum süresi dolmuþ.
	/// </summary>
	SessionExpired = 2,

	/// <summary>
	/// IP adresi deðiþmiþ.
	/// </summary>
	IpMismatch = 3,

	/// <summary>
	/// Browser/Device deðiþmiþ.
	/// </summary>
	DeviceMismatch = 4,

	/// <summary>
	/// GeoLocation þüpheli (imkansýz seyahat).
	/// </summary>
	SuspiciousLocation = 5,

	/// <summary>
	/// Admin tarafýndan iptal edilmiþ.
	/// </summary>
	RevokedByAdmin = 6
}