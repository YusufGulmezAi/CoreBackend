namespace CoreBackend.Application.Common.Interfaces;

/// <summary>
/// Password hashing interface.
/// Şifre hashleme ve doğrulama işlemleri.
/// </summary>
public interface IPasswordHasher
{
	/// <summary>
	/// Şifreyi hashler.
	/// </summary>
	/// <param name="password">Plain text şifre</param>
	/// <returns>Hashlenmiş şifre</returns>
	string HashPassword(string password);

	/// <summary>
	/// Şifreyi doğrular.
	/// </summary>
	/// <param name="password">Plain text şifre</param>
	/// <param name="hashedPassword">Hashlenmiş şifre</param>
	/// <returns>Doğrulama sonucu</returns>
	bool VerifyPassword(string password, string hashedPassword);
}