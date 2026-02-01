using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Settings;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Netgsm SMS servis implementasyonu.
/// </summary>
public class NetgsmSmsService : ISmsService
{
	private readonly NetgsmSettings _settings;
	private readonly HttpClient _httpClient;
	private readonly ILogger<NetgsmSmsService> _logger;

	private const string ApiUrl = "https://api.netgsm.com.tr/sms/send/get";

	public NetgsmSmsService(
		IOptions<NetgsmSettings> settings,
		HttpClient httpClient,
		ILogger<NetgsmSmsService> logger)
	{
		_settings = settings.Value;
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<SmsResult> SendAsync(
		string phoneNumber,
		string message,
		CancellationToken cancellationToken = default)
	{
		try
		{
			// Telefon numarasını formatla (başında 0 varsa kaldır, 90 ile başlamasını sağla)
			var formattedPhone = FormatPhoneNumber(phoneNumber);

			if (string.IsNullOrEmpty(formattedPhone))
			{
				return SmsResult.Failed("INVALID_PHONE", "Invalid phone number format");
			}

			var parameters = new Dictionary<string, string>
			{
				{ "usercode", _settings.UserCode },
				{ "password", _settings.Password },
				{ "gsmno", formattedPhone },
				{ "message", message },
				{ "msgheader", _settings.MessageHeader },
				{ "filter", "0" },
				{ "startdate", "" },
				{ "stopdate", "" }
			};

			var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
			var requestUrl = $"{ApiUrl}?{queryString}";

			var response = await _httpClient.GetStringAsync(requestUrl, cancellationToken);

			// Netgsm response kontrolü
			var result = ParseNetgsmResponse(response);

			if (result.Success)
			{
				_logger.LogInformation("SMS sent to {PhoneNumber}, MessageId: {MessageId}",
					formattedPhone, result.MessageId);
			}
			else
			{
				_logger.LogWarning("SMS failed to {PhoneNumber}: {ErrorCode} - {ErrorMessage}",
					formattedPhone, result.ErrorCode, result.ErrorMessage);
			}

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "SMS send exception to {PhoneNumber}", phoneNumber);
			return SmsResult.Failed("EXCEPTION", ex.Message);
		}
	}

	public async Task<SmsResult> SendTwoFactorCodeAsync(
		string phoneNumber,
		string code,
		int expirationMinutes,
		CancellationToken cancellationToken = default)
	{
		var message = $"Doğrulama kodunuz: {code}. Bu kod {expirationMinutes} dakika geçerlidir.";
		return await SendAsync(phoneNumber, message, cancellationToken);
	}

	private static string? FormatPhoneNumber(string phoneNumber)
	{
		if (string.IsNullOrEmpty(phoneNumber))
			return null;

		// Sadece rakamları al
		var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

		// Türkiye telefon numarası formatı
		if (digits.Length == 10 && digits.StartsWith("5"))
		{
			return "90" + digits;
		}
		if (digits.Length == 11 && digits.StartsWith("05"))
		{
			return "9" + digits.Substring(1);
		}
		if (digits.Length == 12 && digits.StartsWith("90"))
		{
			return digits;
		}

		return null;
	}

	private static SmsResult ParseNetgsmResponse(string response)
	{
		// Netgsm başarılı response: "00 MessageId" veya sadece "00"
		// Hata response'ları: "20", "30", "40" vb.

		var parts = response.Trim().Split(' ');
		var code = parts[0];

		return code switch
		{
			"00" => SmsResult.Succeeded(parts.Length > 1 ? parts[1] : null),
			"01" => SmsResult.Failed("01", "Message header not defined"),
			"20" => SmsResult.Failed("20", "Invalid message text or header"),
			"30" => SmsResult.Failed("30", "Invalid username or password"),
			"40" => SmsResult.Failed("40", "Message header not found in system"),
			"50" => SmsResult.Failed("50", "Insufficient credits"),
			"51" => SmsResult.Failed("51", "Quota exceeded"),
			"60" => SmsResult.Failed("60", "Invalid data"),
			"70" => SmsResult.Failed("70", "Invalid parameters"),
			"80" => SmsResult.Failed("80", "Query limit exceeded"),
			"85" => SmsResult.Failed("85", "Same message sent multiple times"),
			_ => SmsResult.Failed(code, $"Unknown error code: {code}")
		};
	}
}