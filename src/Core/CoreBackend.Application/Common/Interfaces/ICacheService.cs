namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Cache servis interface.
/// Redis veya Memory cache için soyutlama.
/// </summary>
public interface ICacheService
{
	/// <summary>
	/// Cache'ten veri getirir.
	/// </summary>
	Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

	/// <summary>
	/// Cache'e veri yazar.
	/// </summary>
	Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Cache'ten veri siler.
	/// </summary>
	Task RemoveAsync(string key, CancellationToken cancellationToken = default);

	/// <summary>
	/// Belirli bir pattern'e uyan tüm key'leri siler.
	/// </summary>
	Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Key var mı kontrol eder.
	/// </summary>
	Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

	/// <summary>
	/// Key'in süresini uzatır.
	/// </summary>
	Task RefreshAsync(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}