using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services.Caching;

/// <summary>
/// Redis cache servisi (IDistributedCache tabanlı).
/// </summary>
public class RedisCacheService : ICacheService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> _logger;

	public RedisCacheService(
		IDistributedCache cache,
		ILogger<RedisCacheService> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
	{
		try
		{
			var json = await _cache.GetStringAsync(key, cancellationToken);

			if (string.IsNullOrEmpty(json))
				return default;

			return JsonSerializer.Deserialize<T>(json);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting cache key: {Key}", key);
			return default;
		}
	}

	public async Task SetAsync<T>(
		string key,
		T value,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var json = JsonSerializer.Serialize(value);

			var options = new DistributedCacheEntryOptions();

			if (expiration.HasValue)
			{
				options.AbsoluteExpirationRelativeToNow = expiration;
			}
			else
			{
				options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
			}

			await _cache.SetStringAsync(key, json, options, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting cache key: {Key}", key);
		}
	}

	public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
	{
		try
		{
			await _cache.RemoveAsync(key, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing cache key: {Key}", key);
		}
	}

	public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
	{
		try
		{
			var value = await _cache.GetStringAsync(key, cancellationToken);
			return !string.IsNullOrEmpty(value);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking cache key: {Key}", key);
			return false;
		}
	}

	public async Task RefreshAsync(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
	{
		try
		{
			if (expiration.HasValue)
			{
				// Yeni süre ile yeniden kaydet
				var json = await _cache.GetStringAsync(key, cancellationToken);

				if (!string.IsNullOrEmpty(json))
				{
					var options = new DistributedCacheEntryOptions
					{
						AbsoluteExpirationRelativeToNow = expiration
					};

					await _cache.SetStringAsync(key, json, options, cancellationToken);
				}
			}
			else
			{
				// Sadece sliding expiration'ı yenile
				await _cache.RefreshAsync(key, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error refreshing cache key: {Key}", key);
		}
	}

	public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
	{
		// IDistributedCache pattern silme desteklemiyor
		// Bu özellik için IConnectionMultiplexer gerekir
		_logger.LogWarning("RemoveByPrefix is not supported with IDistributedCache. Prefix: {Prefix}", prefixKey);
		await Task.CompletedTask;
	}
}