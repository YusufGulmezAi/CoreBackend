using System.Text.Json;
using CoreBackend.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace CoreBackend.Infrastructure.Services.Caching;

/// <summary>
/// Redis cache servis implementasyonu.
/// </summary>
public class RedisCacheService : ICacheService
{
	private readonly IDistributedCache _distributedCache;
	private readonly IConnectionMultiplexer _connectionMultiplexer;
	private readonly JsonSerializerOptions _jsonOptions;

	public RedisCacheService(
		IDistributedCache distributedCache,
		IConnectionMultiplexer connectionMultiplexer)
	{
		_distributedCache = distributedCache;
		_connectionMultiplexer = connectionMultiplexer;
		_jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};
	}

	/// <summary>
	/// Cache'ten veri getirir.
	/// </summary>
	public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
	{
		var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);

		if (string.IsNullOrEmpty(cachedValue))
			return default;

		try
		{
			return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
		}
		catch
		{
			return default;
		}
	}

	/// <summary>
	/// Cache'e veri yazar.
	/// </summary>
	public async Task SetAsync<T>(
		string key,
		T value,
		TimeSpan? expiration = null,
		CancellationToken cancellationToken = default)
	{
		var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

		var options = new DistributedCacheEntryOptions();

		if (expiration.HasValue)
		{
			options.AbsoluteExpirationRelativeToNow = expiration;
		}
		else
		{
			// Varsayılan 1 saat
			options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
		}

		await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
	}

	/// <summary>
	/// Cache'ten veri siler.
	/// </summary>
	public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
	{
		await _distributedCache.RemoveAsync(key, cancellationToken);
	}

	/// <summary>
	/// Belirli bir pattern'e uyan tüm key'leri siler.
	/// </summary>
	public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
	{
		var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
		var database = _connectionMultiplexer.GetDatabase();

		var keys = server.Keys(pattern: $"{prefixKey}*").ToArray();

		if (keys.Any())
		{
			await database.KeyDeleteAsync(keys);
		}
	}

	/// <summary>
	/// Key var mı kontrol eder.
	/// </summary>
	public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
	{
		var value = await _distributedCache.GetAsync(key, cancellationToken);
		return value != null;
	}

	/// <summary>
	/// Key'in süresini uzatır.
	/// </summary>
	public async Task RefreshAsync(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
	{
		var value = await _distributedCache.GetStringAsync(key, cancellationToken);

		if (!string.IsNullOrEmpty(value))
		{
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
			};

			await _distributedCache.SetStringAsync(key, value, options, cancellationToken);
		}
	}
}