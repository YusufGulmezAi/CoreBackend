using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Settings;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Redis tabanlı user session servisi.
/// </summary>
public class UserSessionService : IUserSessionService
{
	private readonly IDistributedCache _cache;
	private readonly SessionSettings _settings;
	private readonly ILogger<UserSessionService> _logger;

	private const string SessionPrefix = "session:";
	private const string UserSessionsPrefix = "user_sessions:";

	public UserSessionService(
		IDistributedCache cache,
		IOptions<SessionSettings> settings,
		ILogger<UserSessionService> logger)
	{
		_cache = cache;
		_settings = settings.Value;
		_logger = logger;
	}

	public async Task<string> CreateSessionAsync(
		UserSessionData sessionData,
		CancellationToken cancellationToken = default)
	{
		var sessionId = Guid.NewGuid().ToString("N");
		var sessionKey = SessionPrefix + sessionId;
		var userSessionsKey = UserSessionsPrefix + sessionData.UserId;

		var json = JsonSerializer.Serialize(sessionData);

		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpiration = sessionData.ExpiresAt
		};

		await _cache.SetStringAsync(sessionKey, json, options, cancellationToken);

		// Kullanıcının session listesine ekle
		await AddSessionToUserListAsync(userSessionsKey, sessionId, sessionData.ExpiresAt, cancellationToken);

		_logger.LogInformation("Session created: {SessionId} for User: {UserId}", sessionId, sessionData.UserId);

		return sessionId;
	}

	public async Task<UserSessionData?> GetSessionAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		var sessionKey = SessionPrefix + sessionId;
		var json = await _cache.GetStringAsync(sessionKey, cancellationToken);

		if (string.IsNullOrEmpty(json))
			return null;

		return JsonSerializer.Deserialize<UserSessionData>(json);
	}

	public async Task UpdateSessionAsync(
		string sessionId,
		UserSessionData sessionData,
		CancellationToken cancellationToken = default)
	{
		var sessionKey = SessionPrefix + sessionId;
		var json = JsonSerializer.Serialize(sessionData);

		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpiration = sessionData.ExpiresAt
		};

		await _cache.SetStringAsync(sessionKey, json, options, cancellationToken);
	}

	public async Task RevokeSessionAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		var sessionKey = SessionPrefix + sessionId;

		// Önce session'ı al (userId için)
		var session = await GetSessionAsync(sessionId, cancellationToken);

		await _cache.RemoveAsync(sessionKey, cancellationToken);

		// Kullanıcının session listesinden kaldır
		if (session != null)
		{
			var userSessionsKey = UserSessionsPrefix + session.UserId;
			await RemoveSessionFromUserListAsync(userSessionsKey, sessionId, cancellationToken);
		}

		_logger.LogInformation("Session revoked: {SessionId}", sessionId);
	}

	public async Task RevokeAllUserSessionsAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var userSessionsKey = UserSessionsPrefix + userId;
		var sessionIds = await GetUserSessionIdsAsync(userSessionsKey, cancellationToken);

		foreach (var sessionId in sessionIds)
		{
			var sessionKey = SessionPrefix + sessionId;
			await _cache.RemoveAsync(sessionKey, cancellationToken);
		}

		// Kullanıcının session listesini temizle
		await _cache.RemoveAsync(userSessionsKey, cancellationToken);

		_logger.LogInformation("All sessions revoked for User: {UserId}, Count: {Count}", userId, sessionIds.Count);
	}

	public async Task<IReadOnlyList<UserSessionData>> GetUserActiveSessionsAsync(
		Guid userId,
		CancellationToken cancellationToken = default)
	{
		var userSessionsKey = UserSessionsPrefix + userId;
		var sessionIds = await GetUserSessionIdsAsync(userSessionsKey, cancellationToken);

		var sessions = new List<UserSessionData>();

		foreach (var sessionId in sessionIds)
		{
			var session = await GetSessionAsync(sessionId, cancellationToken);
			if (session != null && session.ExpiresAt > DateTime.UtcNow)
			{
				sessions.Add(session);
			}
		}

		return sessions;
	}

	public async Task<bool> ValidateSessionAsync(
		string sessionId,
		string? ipAddress = null,
		string? userAgent = null,
		CancellationToken cancellationToken = default)
	{
		var session = await GetSessionAsync(sessionId, cancellationToken);

		if (session == null)
			return false;

		// Süre kontrolü
		if (session.ExpiresAt < DateTime.UtcNow)
		{
			await RevokeSessionAsync(sessionId, cancellationToken);
			return false;
		}

		// IP kontrolü
		if (!session.AllowIpChange && !string.IsNullOrEmpty(ipAddress) && session.IpAddress != ipAddress)
		{
			_logger.LogWarning("Session IP mismatch: {SessionId}, Expected: {Expected}, Actual: {Actual}",
				sessionId, session.IpAddress, ipAddress);

			if (!_settings.AllowIpChange)
				return false;
		}

		// User Agent kontrolü
		if (!session.AllowUserAgentChange && !string.IsNullOrEmpty(userAgent) && session.UserAgent != userAgent)
		{
			_logger.LogWarning("Session UserAgent mismatch: {SessionId}", sessionId);

			if (!_settings.AllowUserAgentChange)
				return false;
		}

		return true;
	}

	public async Task RefreshSessionActivityAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		var session = await GetSessionAsync(sessionId, cancellationToken);

		if (session == null)
			return;

		session.LastActivityAt = DateTime.UtcNow;
		await UpdateSessionAsync(sessionId, session, cancellationToken);
	}

	#region Private Helpers

	private async Task AddSessionToUserListAsync(
		string userSessionsKey,
		string sessionId,
		DateTime expiresAt,
		CancellationToken cancellationToken)
	{
		var sessionIds = await GetUserSessionIdsAsync(userSessionsKey, cancellationToken);
		sessionIds.Add(sessionId);

		var json = JsonSerializer.Serialize(sessionIds);
		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpiration = expiresAt.AddDays(1) // Session listesi biraz daha uzun yaşasın
		};

		await _cache.SetStringAsync(userSessionsKey, json, options, cancellationToken);
	}

	private async Task RemoveSessionFromUserListAsync(
		string userSessionsKey,
		string sessionId,
		CancellationToken cancellationToken)
	{
		var sessionIds = await GetUserSessionIdsAsync(userSessionsKey, cancellationToken);
		sessionIds.Remove(sessionId);

		if (sessionIds.Any())
		{
			var json = JsonSerializer.Serialize(sessionIds);
			await _cache.SetStringAsync(userSessionsKey, json, cancellationToken);
		}
		else
		{
			await _cache.RemoveAsync(userSessionsKey, cancellationToken);
		}
	}

	private async Task<List<string>> GetUserSessionIdsAsync(
		string userSessionsKey,
		CancellationToken cancellationToken)
	{
		var json = await _cache.GetStringAsync(userSessionsKey, cancellationToken);

		if (string.IsNullOrEmpty(json))
			return new List<string>();

		return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
	}

	#endregion
}