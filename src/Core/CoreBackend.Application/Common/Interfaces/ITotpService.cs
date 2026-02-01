namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// TOTP (Time-based One-Time Password) servis interface.
/// </summary>
public interface ITotpService
{
	/// <summary>
	/// Yeni TOTP secret key oluşturur.
	/// </summary>
	string GenerateSecretKey();

	/// <summary>
	/// QR kod URI'si oluşturur.
	/// </summary>
	string GenerateQrCodeUri(string email, string secretKey, string issuer = "CoreBackend");

	/// <summary>
	/// TOTP kodunu doğrular.
	/// </summary>
	bool VerifyCode(string secretKey, string code);

	/// <summary>
	/// Mevcut TOTP kodunu oluşturur.
	/// </summary>
	string GenerateCode(string secretKey);
}