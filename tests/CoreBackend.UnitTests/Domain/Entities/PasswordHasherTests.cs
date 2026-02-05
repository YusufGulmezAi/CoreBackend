using CoreBackend.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CoreBackend.UnitTests.Infrastructure.Services;

/// <summary>
/// PasswordHasher unit testleri.
/// </summary>
public class PasswordHasherTests
{
	private readonly PasswordHasher _sut = new();

	[Fact]
	public void HashPassword_ShouldReturnHashedPassword()
	{
		// Arrange
		var password = "TestPassword123!";

		// Act
		var hash = _sut.HashPassword(password);

		// Assert
		hash.Should().NotBeNullOrEmpty();
		hash.Should().NotBe(password);
		hash.Should().Contain(";"); // PBKDF2 format uses semicolon delimiter
	}

	[Fact]
	public void HashPassword_SamePasswordTwice_ShouldReturnDifferentHashes()
	{
		// Arrange
		var password = "TestPassword123!";

		// Act
		var hash1 = _sut.HashPassword(password);
		var hash2 = _sut.HashPassword(password);

		// Assert
		hash1.Should().NotBe(hash2); // Different salt each time
	}

	[Fact]
	public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
	{
		// Arrange
		var password = "TestPassword123!";
		var hash = _sut.HashPassword(password);

		// Act
		var result = _sut.VerifyPassword(password, hash);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
	{
		// Arrange
		var password = "TestPassword123!";
		var hash = _sut.HashPassword(password);

		// Act
		var result = _sut.VerifyPassword("WrongPassword123!", hash);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
	{
		// Act
		var result = _sut.VerifyPassword("password", "invalid-hash");

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void VerifyPassword_WithEmptyHash_ShouldReturnFalse()
	{
		// Act
		var result = _sut.VerifyPassword("password", "");

		// Assert
		result.Should().BeFalse();
	}
}