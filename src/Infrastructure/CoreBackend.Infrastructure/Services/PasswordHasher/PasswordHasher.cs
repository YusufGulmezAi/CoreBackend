using System.Security.Cryptography;
using CoreBackend.Application.Common.Interfaces;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Password hasher implementasyonu.
/// PBKDF2 algoritması kullanır.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
	private const int SaltSize = 16; // 128 bit
	private const int KeySize = 32; // 256 bit
	private const int Iterations = 100000;
	private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

	private const char Delimiter = ';';

	/// <summary>
	/// Şifreyi hashler.
	/// </summary>
	public string HashPassword(string password)
	{
		var salt = RandomNumberGenerator.GetBytes(SaltSize);
		var hash = Rfc2898DeriveBytes.Pbkdf2(
			password,
			salt,
			Iterations,
			Algorithm,
			KeySize);

		return string.Join(
			Delimiter,
			Convert.ToBase64String(salt),
			Convert.ToBase64String(hash),
			Iterations,
			Algorithm.Name);
	}

	/// <summary>
	/// Şifreyi doğrular.
	/// </summary>
	public bool VerifyPassword(string password, string hashedPassword)
	{
		try
		{
			var parts = hashedPassword.Split(Delimiter);
			if (parts.Length != 4)
				return false;

			var salt = Convert.FromBase64String(parts[0]);
			var hash = Convert.FromBase64String(parts[1]);
			var iterations = int.Parse(parts[2]);
			var algorithmName = new HashAlgorithmName(parts[3]);

			var testHash = Rfc2898DeriveBytes.Pbkdf2(
				password,
				salt,
				iterations,
				algorithmName,
				hash.Length);

			return CryptographicOperations.FixedTimeEquals(hash, testHash);
		}
		catch
		{
			return false;
		}
	}
}