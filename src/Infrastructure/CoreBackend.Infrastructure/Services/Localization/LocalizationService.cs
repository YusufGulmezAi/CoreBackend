using System.Globalization;
using System.Resources;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services.Localization;

/// <summary>
/// Localization servis implementasyonu.
/// Hata kodlarına göre çevrilmiş mesajları döner.
/// </summary>
public class LocalizationService : ILocalizationService
{
	private readonly ResourceManager _resourceManager;

	public LocalizationService()
	{
		// Resource dosyasının tam yolu
		_resourceManager = new ResourceManager(
			"CoreBackend.Application.Resources.ErrorMessages.ErrorMessages",
			typeof(CoreBackend.Application.DependencyInjection).Assembly);
	}

	/// <summary>
	/// Hata koduna göre çevrilmiş mesajı döner.
	/// </summary>
	public string GetMessage(string errorCode)
	{
		try
		{
			var message = _resourceManager.GetString(errorCode, CultureInfo.CurrentUICulture);
			return message ?? errorCode;
		}
		catch
		{
			return errorCode;
		}
	}

	/// <summary>
	/// Hata koduna göre parametreli çevrilmiş mesajı döner.
	/// </summary>
	public string GetMessage(string errorCode, params object[] parameters)
	{
		var message = GetMessage(errorCode);

		try
		{
			return string.Format(message, parameters);
		}
		catch
		{
			return message;
		}
	}

	/// <summary>
	/// Hata koduna göre parametreli çevrilmiş mesajı döner (named parameters).
	/// </summary>
	public string GetMessage(string errorCode, object parameters)
	{
		var message = GetMessage(errorCode);

		if (parameters == null)
			return message;

		try
		{
			var properties = parameters.GetType().GetProperties();

			foreach (var prop in properties)
			{
				var placeholder = $"{{{prop.Name}}}";
				var value = prop.GetValue(parameters)?.ToString() ?? string.Empty;
				message = message.Replace(placeholder, value);
			}

			return message;
		}
		catch
		{
			return message;
		}
	}

	/// <summary>
	/// Belirtilen dilde mesajı döner.
	/// </summary>
	public string GetMessage(string errorCode, string culture)
	{
		try
		{
			var cultureInfo = new CultureInfo(culture);
			var message = _resourceManager.GetString(errorCode, cultureInfo);
			return message ?? errorCode;
		}
		catch
		{
			return errorCode;
		}
	}
}