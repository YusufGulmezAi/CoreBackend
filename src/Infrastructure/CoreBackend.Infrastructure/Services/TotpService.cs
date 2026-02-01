using System.Security.Cryptography;
using System.Text;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// TOTP servis implementasyonu.
/// RFC 6238 standardına uygun.
/// </summary>
public class TotpService : ITotpService
{
	private const int SecretKeyLength = 20;
	private const int CodeDigits = 6;
	private const int TimeStepSeconds = 30;
	private const int AllowedTimeStepDrift = 1; // ±1 step tolerance

	/// <summary>
	/// Yeni TOTP secret key oluşturur.
	/// </summary>
	public string GenerateSecretKey()
	{
		var key = new byte[SecretKeyLength];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(key);
		return Base32Encode(key);
	}

	/// <summary>
	/// QR kod URI'si oluşturur.
	/// </summary>
	public string GenerateQrCodeUri(string email, string secretKey, string issuer = "CoreBackend")
	{
		var encodedIssuer = Uri.EscapeDataString(issuer);
		var encodedEmail = Uri.EscapeDataString(email);
		return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits={CodeDigits}&period={TimeStepSeconds}";
	}

	/// <summary>
	/// TOTP kodunu doğrular.
	/// </summary>
	public bool VerifyCode(string secretKey, string code)
	{
		if (string.IsNullOrEmpty(code) || code.Length != CodeDigits)
			return false;

		var key = Base32Decode(secretKey);
		var currentTimeStep = GetCurrentTimeStep();

		// ±1 time step tolerance
		for (int i = -AllowedTimeStepDrift; i <= AllowedTimeStepDrift; i++)
		{
			var expectedCode = GenerateCodeForTimeStep(key, currentTimeStep + i);
			if (code == expectedCode)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Mevcut TOTP kodunu oluşturur.
	/// </summary>
	public string GenerateCode(string secretKey)
	{
		var key = Base32Decode(secretKey);
		var timeStep = GetCurrentTimeStep();
		return GenerateCodeForTimeStep(key, timeStep);
	}

	private static long GetCurrentTimeStep()
	{
		var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		return unixTimestamp / TimeStepSeconds;
	}

	private static string GenerateCodeForTimeStep(byte[] key, long timeStep)
	{
		var timeStepBytes = BitConverter.GetBytes(timeStep);
		if (BitConverter.IsLittleEndian)
			Array.Reverse(timeStepBytes);

		// Pad to 8 bytes
		var data = new byte[8];
		Array.Copy(timeStepBytes, 0, data, 8 - timeStepBytes.Length, timeStepBytes.Length);

		using var hmac = new HMACSHA1(key);
		var hash = hmac.ComputeHash(data);

		// Dynamic truncation
		var offset = hash[hash.Length - 1] & 0x0F;
		var binaryCode = (hash[offset] & 0x7F) << 24
			| (hash[offset + 1] & 0xFF) << 16
			| (hash[offset + 2] & 0xFF) << 8
			| (hash[offset + 3] & 0xFF);

		var code = binaryCode % (int)Math.Pow(10, CodeDigits);
		return code.ToString().PadLeft(CodeDigits, '0');
	}

	private static string Base32Encode(byte[] data)
	{
		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		var output = new StringBuilder();

		for (int bitIndex = 0; bitIndex < data.Length * 8; bitIndex += 5)
		{
			int byteIndex = bitIndex / 8;
			int shift = 3 - (bitIndex % 8);

			int value;
			if (shift >= 0)
			{
				value = (data[byteIndex] >> shift) & 0x1F;
			}
			else
			{
				value = (data[byteIndex] << -shift) & 0x1F;
				if (byteIndex + 1 < data.Length)
					value |= data[byteIndex + 1] >> (8 + shift);
			}

			output.Append(alphabet[value]);
		}

		return output.ToString();
	}

	private static byte[] Base32Decode(string input)
	{
		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		input = input.ToUpperInvariant().TrimEnd('=');

		var output = new byte[input.Length * 5 / 8];
		var bitIndex = 0;
		var inputIndex = 0;

		while (inputIndex < input.Length)
		{
			var charValue = alphabet.IndexOf(input[inputIndex]);
			if (charValue < 0)
				throw new ArgumentException("Invalid Base32 character");

			var byteIndex = bitIndex / 8;
			var shift = 3 - (bitIndex % 8);

			if (shift >= 0)
			{
				output[byteIndex] |= (byte)(charValue << shift);
			}
			else
			{
				output[byteIndex] |= (byte)(charValue >> -shift);
				if (byteIndex + 1 < output.Length)
					output[byteIndex + 1] |= (byte)(charValue << (8 + shift));
			}

			bitIndex += 5;
			inputIndex++;
		}

		return output;
	}
}