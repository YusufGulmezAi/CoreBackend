namespace CoreBackend.Application.Common.Models.Session;

/// <summary>
/// Admin paneli için oturum özeti.
/// </summary>
public sealed record UserSessionSummary
{
	/// <summary>
	/// Oturum Id.
	/// </summary>
	public required string SessionId { get; init; }

	/// <summary>
	/// Cihaz bilgisi.
	/// </summary>
	public required DeviceInfo Device { get; init; }

	/// <summary>
	/// Oturum oluþturulma zamaný.
	/// </summary>
	public required DateTime CreatedAt { get; init; }

	/// <summary>
	/// Son aktivite zamaný.
	/// </summary>
	public required DateTime LastActivityAt { get; init; }

	/// <summary>
	/// Oturum son kullanma tarihi.
	/// </summary>
	public required DateTime ExpiresAt { get; init; }

	/// <summary>
	/// Mevcut oturum mu?
	/// </summary>
	public required bool IsCurrentSession { get; init; }
}