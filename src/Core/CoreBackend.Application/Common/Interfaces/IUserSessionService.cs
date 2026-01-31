using CoreBackend.Application.Common.Models.Session;

namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Kullanýcý oturum yönetimi.
/// Cache'teki session bilgilerini yönetir.
/// </summary>
public interface IUserSessionService
{
	/// <summary>
	/// Oturum bilgilerini cache'ten getirir.
	/// </summary>
	Task<UserSessionData?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Yeni oturum oluþturur ve cache'e kaydeder.
	/// </summary>
	Task<string> CreateSessionAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Oturumu sonlandýrýr (cache'ten siler).
	/// </summary>
	Task InvalidateSessionAsync(string sessionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Kullanýcýnýn tüm oturumlarýný sonlandýrýr.
	/// </summary>
	Task InvalidateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Oturumdaki rol/izin bilgilerini günceller.
	/// </summary>
	Task RefreshPermissionsAsync(string sessionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Mevcut request'in device bilgisi session ile uyumlu mu?
	/// </summary>
	Task<SessionValidationResult> ValidateSessionSecurityAsync(
		string sessionId,
		DeviceInfo currentDevice,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Kullanýcýnýn aktif oturum sayýsýný getirir.
	/// </summary>
	Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Kullanýcýnýn tüm aktif oturumlarýný listeler (Admin için).
	/// </summary>
	Task<IReadOnlyList<UserSessionSummary>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Son aktivite zamanýný günceller.
	/// </summary>
	Task UpdateLastActivityAsync(string sessionId, CancellationToken cancellationToken = default);
}