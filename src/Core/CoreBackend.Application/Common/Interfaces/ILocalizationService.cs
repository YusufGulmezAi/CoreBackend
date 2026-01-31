namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Localization servis interface.
/// Hata kodlarına göre çevrilmiş mesajları döner.
/// </summary>
public interface ILocalizationService
{
	/// <summary>
	/// Hata koduna göre çevrilmiş mesajı döner.
	/// </summary>
	/// <param name="errorCode">Hata kodu (örn: GENERAL_NOT_FOUND)</param>
	/// <returns>Çevrilmiş mesaj</returns>
	string GetMessage(string errorCode);

	/// <summary>
	/// Hata koduna göre parametreli çevrilmiş mesajı döner.
	/// </summary>
	/// <param name="errorCode">Hata kodu</param>
	/// <param name="parameters">Mesaj parametreleri</param>
	/// <returns>Çevrilmiş ve formatlanmış mesaj</returns>
	string GetMessage(string errorCode, params object[] parameters);

	/// <summary>
	/// Hata koduna göre parametreli çevrilmiş mesajı döner.
	/// </summary>
	/// <param name="errorCode">Hata kodu</param>
	/// <param name="parameters">Mesaj parametreleri (named)</param>
	/// <returns>Çevrilmiş ve formatlanmış mesaj</returns>
	string GetMessage(string errorCode, object parameters);

	/// <summary>
	/// Belirtilen dilde mesajı döner.
	/// </summary>
	/// <param name="errorCode">Hata kodu</param>
	/// <param name="culture">Kültür kodu (örn: tr-TR, en-US)</param>
	/// <returns>Çevrilmiş mesaj</returns>
	string GetMessage(string errorCode, string culture);
}