using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// User session servis implementasyonu.
/// Redis'te session verilerini yönetir.
/// </summary>
public class UserSessionService : IUserSessionService
{
	private readonly ICacheService _cacheService;
	private const string SessionPrefix = "session:";
	private const string UserSessionsPrefix = "user_sessions:";
	private const string TenantSessionsPrefix = "tenant_sessions:";

	public UserSessionService(ICacheService cacheService)
	{
		_cacheService = cacheService;
	}

	/// <summary>
	/// Session bilgilerini cache'ten getirir.
	/// </summary>
	public async Task<UserSessionData?> GetSessionAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		var key = $"{SessionPrefix}{sessionId}";
		return await _cacheService.GetAsync<UserSessionData>(key, cancellationToken);
	}

	/// <summary>
	/// Yeni session oluşturur ve cache'e kaydeder.
	/// </summary>
	public async Task<string> CreateSessionAsync(
		UserSessionData sessionData,
		CancellationToken cancellationToken = default)
	{
		var sessionId = Guid.NewGuid().ToString("N");
		var expiration = sessionData.ExpiresAt - DateTime.UtcNow;

		// Session verisini kaydet
		var sessionKey = $"{SessionPrefix}{sessionId}";
		await _cacheService.SetAsync(sessionKey, sessionData, expiration, cancellationToken);

		// Kullanıcının session listesine ekle
		var userSessionsKey = $"{UserSessionsPrefix}{sessionData.UserId}";
		var userSessions = await _cacheService.GetAsync<List<string>>(userSessionsKey, cancellationToken) ?? new List<string>();
		userSessions.Add(sessionId);
		await _cacheService.SetAsync(userSessionsKey, userSessions, TimeSpan.FromDays(30), cancellationToken);

		// Tenant'ın session listesine ekle
		var tenantSessionsKey = $"{TenantSessionsPrefix}{sessionData.TenantId}";
		var tenantSessions = await _cacheService.GetAsync<List<string>>(tenantSessionsKey, cancellationToken) ?? new List<string>();
		tenantSessions.Add(sessionId);
		await _cacheService.SetAsync(tenantSessionsKey, tenantSessions, TimeSpan.FromDays(30), cancellationToken);

		return sessionId;
	}

	/// <summary>
	/// Session bilgilerini günceller.
	/// </summary>
	public async Task UpdateSessionAsync(
		string sessionId,
		UserSessionData sessionData,
		CancellationToken cancellationToken = default)
	{
		var key = $"{SessionPrefix}{sessionId}";
		var expiration = sessionData.ExpiresAt - DateTime.UtcNow;

		if (expiration > TimeSpan.Zero)
		{
			await _cacheService.SetAsync(key, sessionData, expiration, cancellationToken);
		}
	}

	/// <summary>
	/// Session'ı sonlandırır (cache'ten siler).
	/// </summary>
	public async Task RevokeSessionAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		// Session verisini al
		var session = await GetSessionAsync(sessionId, cancellationToken);

		// Session'ı sil
		var sessionKey = $"{SessionPrefix}{sessionId}";
		await _cacheService.RemoveAsync(sessionKey, cancellationToken);

		if (session != null)
		{
			// Kullanıcının session listesinden çıkar
			await RemoveFromUserSessionsAsync(session.UserId, sessionId, cancellationToken);

			// Tenant'ın session listesinden çıkar
			await RemoveFromTenantSessionsAsync(session.TenantId, sessionId, cancellationToken);
		}
	}

	/// <summary>
	/// Kullanıcının tüm session'larını sonlandırır.
	/// </summary>
	public async Task RevokeAllUserSessionsAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var userSessionsKey = $"{UserSessionsPrefix}{userId}";
		var sessionIds = await _cacheService.GetAsync<List<string>>(userSessionsKey, cancellationToken);

		if (sessionIds != null)
		{
			foreach (var sessionId in sessionIds)
			{
				var sessionKey = $"{SessionPrefix}{sessionId}";
				await _cacheService.RemoveAsync(sessionKey, cancellationToken);
			}
		}

		await _cacheService.RemoveAsync(userSessionsKey, cancellationToken);
	}

	/// <summary>
	/// Tenant'ın tüm session'larını sonlandırır.
	/// </summary>
	public async Task RevokeAllTenantSessionsAsync(
		Guid tenantId,
		CancellationToken cancellationToken = default)
	{
		var tenantSessionsKey = $"{TenantSessionsPrefix}{tenantId}";
		var sessionIds = await _cacheService.GetAsync<List<string>>(tenantSessionsKey, cancellationToken);

		if (sessionIds != null)
		{
			foreach (var sessionId in sessionIds)
			{
				var sessionKey = $"{SessionPrefix}{sessionId}";
				await _cacheService.RemoveAsync(sessionKey, cancellationToken);
			}
		}

		await _cacheService.RemoveAsync(tenantSessionsKey, cancellationToken);
	}

	/// <summary>
	/// Session'ın geçerli olup olmadığını kontrol eder.
	/// </summary>
	public async Task<SessionValidationResult> ValidateSessionAsync(
		string sessionId,
		string currentIp,
		string currentUserAgent,
		CancellationToken cancellationToken = default)
	{
		var session = await GetSessionAsync(sessionId, cancellationToken);

		if (session == null)
		{
			return SessionValidationResult.Failed(
				ErrorCodes.Auth.TokenInvalid,
				"Session not found.");
		}

		// Süre kontrolü
		if (DateTime.UtcNow > session.ExpiresAt)
		{
			await RevokeSessionAsync(sessionId, cancellationToken);
			return SessionValidationResult.Failed(
				ErrorCodes.Auth.TokenExpired,
				"Session has expired.");
		}

		// IP kontrolü
		if (!session.AllowIpChange && session.IpAddress != currentIp)
		{
			return SessionValidationResult.Failed(
				ErrorCodes.Auth.TokenInvalid,
				"IP address mismatch.");
		}

		// User Agent kontrolü
		if (!session.AllowUserAgentChange && session.UserAgent != currentUserAgent)
		{
			return SessionValidationResult.Failed(
				ErrorCodes.Auth.TokenInvalid,
				"User agent mismatch.");
		}

		// Son aktivite güncelle
		session.LastActivityAt = DateTime.UtcNow;
		await UpdateSessionAsync(sessionId, session, cancellationToken);

		return SessionValidationResult.Success(session);
	}

	/// <summary>
	/// Kullanıcının aktif session'larını listeler.
	/// </summary>
	public async Task<IReadOnlyList<UserSessionData>> GetUserActiveSessionsAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var userSessionsKey = $"{UserSessionsPrefix}{userId}";
		var sessionIds = await _cacheService.GetAsync<List<string>>(userSessionsKey, cancellationToken);

		if (sessionIds == null || !sessionIds.Any())
		{
			return Array.Empty<UserSessionData>();
		}

		var activeSessions = new List<UserSessionData>();

		foreach (var sessionId in sessionIds)
		{
			var session = await GetSessionAsync(sessionId, cancellationToken);
			if (session != null && DateTime.UtcNow < session.ExpiresAt)
			{
				activeSessions.Add(session);
			}
		}

		return activeSessions;
	}

	/// <summary>
	/// Session'daki rol ve izinleri günceller.
	/// </summary>
	public async Task RefreshSessionPermissionsAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		// Bu metod normalde veritabanından rolleri ve izinleri çekip
		// session'ı güncelleyecek. Şimdilik placeholder.
		var session = await GetSessionAsync(sessionId, cancellationToken);

		if (session != null)
		{
			// TODO: Veritabanından güncel rol ve izinleri çek
			// session.TenantRoles = ...
			// session.CompanyRoles = ...
			// session.Permissions = ...

			await UpdateSessionAsync(sessionId, session, cancellationToken);
		}
	}

	/// <summary>
	/// Kullanıcının session listesinden çıkarır.
	/// </summary>
	private async Task RemoveFromUserSessionsAsync(
		Guid userId,
		string sessionId,
		CancellationToken cancellationToken)
	{
		var userSessionsKey = $"{UserSessionsPrefix}{userId}";
		var sessions = await _cacheService.GetAsync<List<string>>(userSessionsKey, cancellationToken);

		if (sessions != null)
		{
			sessions.Remove(sessionId);
			await _cacheService.SetAsync(userSessionsKey, sessions, TimeSpan.FromDays(30), cancellationToken);
		}
	}

	/// <summary>
	/// Tenant'ın session listesinden çıkarır.
	/// </summary>
	private async Task RemoveFromTenantSessionsAsync(
		Guid tenantId,
		string sessionId,
		CancellationToken cancellationToken)
	{
		var tenantSessionsKey = $"{TenantSessionsPrefix}{tenantId}";
		var sessions = await _cacheService.GetAsync<List<string>>(tenantSessionsKey, cancellationToken);

		if (sessions != null)
		{
			sessions.Remove(sessionId);
			await _cacheService.SetAsync(tenantSessionsKey, sessions, TimeSpan.FromDays(30), cancellationToken);
		}
	}
}