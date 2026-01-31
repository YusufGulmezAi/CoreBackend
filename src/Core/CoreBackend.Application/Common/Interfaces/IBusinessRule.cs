namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Business rule interface.
/// Tüm iş kuralları bu interface'i implemente eder.
/// </summary>
public interface IBusinessRule
{
	/// <summary>
	/// Kural ihlal edildi mi?
	/// </summary>
	Task<bool> IsBrokenAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Hata kodu.
	/// </summary>
	string ErrorCode { get; }

	/// <summary>
	/// Hata mesajı (varsayılan İngilizce).
	/// </summary>
	string ErrorMessage { get; }

	/// <summary>
	/// Hata parametreleri (çok dilli mesajlar için).
	/// </summary>
	object? Parameters { get; }
}