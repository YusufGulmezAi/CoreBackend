using OtpNet;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// TOTP servis implementasyonu (OTP.NET tabanlı).
/// RFC 6238 uyumlu.
/// </summary>
public class TotpService : ITotpService
{
	private const int SecretKeyLength = 20;
	private const int CodeDigits = 6;
	private const int PeriodSeconds = 30;

	/// <summary>
	/// Yeni TOTP secret key oluşturur.
	/// </summary>
	public string GenerateSecretKey()
	{
		var key = KeyGeneration.GenerateRandomKey(SecretKeyLength);
		return Base32Encoding.ToString(key);
	}

	/// <summary>
	/// QR kod URI'si oluşturur (Google Authenticator, Authy vb. için).
	/// </summary>
	public string GenerateQrCodeUri(string email, string secretKey, string issuer = "CoreBackend")
	{
		var encodedIssuer = Uri.EscapeDataString(issuer);
		var encodedEmail = Uri.EscapeDataString(email);
		return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeDigits}&period={PeriodSeconds}";
	}

	/// <summary>
	/// TOTP kodunu doğrular.
	/// </summary>
	public bool VerifyCode(string secretKey, string code)
	{
		if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code))
			return false;

		if (code.Length != CodeDigits)
			return false;

		try
		{
			var key = Base32Encoding.ToBytes(secretKey);
			var totp = new Totp(key, step: PeriodSeconds, totpSize: CodeDigits);

			// VerificationWindow.RfcSpecifiedNetworkDelay = ±1 step (30 saniye tolerans)
			return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Mevcut TOTP kodunu oluşturur.
	/// </summary>
	public string GenerateCode(string secretKey)
	{
		try
		{
			var key = Base32Encoding.ToBytes(secretKey);
			var totp = new Totp(key, step: PeriodSeconds, totpSize: CodeDigits);
			return totp.ComputeTotp();
		}
		catch
		{
			return string.Empty;
		}
	}
}