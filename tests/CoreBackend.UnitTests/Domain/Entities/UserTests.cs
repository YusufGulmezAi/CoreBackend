using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace CoreBackend.UnitTests.Domain.Entities;

/// <summary>
/// User entity testleri.
/// 
/// TEST KATEGORİLERİ:
/// ------------------
/// 1. Create: Kullanıcı oluşturma
/// 2. Profile: Profil güncelleme
/// 3. Email: Email işlemleri
/// 4. Password: Şifre işlemleri
/// 5. Login: Giriş denemeleri ve hesap kilitleme
/// 6. Status: Hesap durumu değişiklikleri
/// 7. RefreshToken: Token yönetimi
/// 8. TwoFactor: 2FA işlemleri
/// 9. RecoveryCodes: Kurtarma kodları
/// 
/// İŞ KURALLARI:
/// -------------
/// - Yeni kullanıcılar PendingVerification durumunda başlar
/// - 5 başarısız giriş hesabı kilitler (15 dk)
/// - Email değişince EmailConfirmed false olur
/// - Email doğrulanınca PendingVerification -> Active olur
/// </summary>
/// <summary>
/// User entity unit testleri.
/// </summary>
/// <summary>
/// User entity unit testleri.
/// </summary>
public class UserTests
{
	private readonly Guid _tenantId = Guid.NewGuid();

	[Fact]
	public void Create_WithValidData_ShouldCreateUser()
	{
		// Act
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hashedPassword", "John", "Doe", "+905551234567");

		// Assert
		user.Should().NotBeNull();
		user.TenantId.Should().Be(_tenantId);
		user.Username.Should().Be("testuser");
		user.Email.Should().Be("test@email.com");
		user.FirstName.Should().Be("John");
		user.LastName.Should().Be("Doe");
		user.Status.Should().Be(UserStatus.PendingVerification); // Düzeltildi
		user.EmailConfirmed.Should().BeFalse();
	}

	[Fact]
	public void FullName_ShouldReturnCombinedName()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");

		// Assert
		user.FullName.Should().Be("John Doe");
	}

	[Fact]
	public void Activate_ShouldSetStatusToActive()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");

		// Act
		user.Activate();

		// Assert
		user.Status.Should().Be(UserStatus.Active);
	}

	[Fact]
	public void Deactivate_ShouldSetStatusToInactive()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");
		user.Activate();

		// Act
		user.Deactivate();

		// Assert
		user.Status.Should().Be(UserStatus.Inactive);
	}

	[Fact]
	public void ConfirmEmail_ShouldSetEmailConfirmedToTrue()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");

		// Act
		user.ConfirmEmail();

		// Assert
		user.EmailConfirmed.Should().BeTrue();
	}

	[Fact]
	public void ConfirmEmail_WhenPendingVerification_ShouldActivateUser()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");
		user.Status.Should().Be(UserStatus.PendingVerification);

		// Act
		user.ConfirmEmail();

		// Assert
		user.EmailConfirmed.Should().BeTrue();
		user.Status.Should().Be(UserStatus.Active);
	}

	[Fact]
	public void RecordFailedLogin_ShouldIncrementFailedAttempts()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");

		// Act
		user.RecordFailedLogin();
		user.RecordFailedLogin();

		// Assert
		user.FailedLoginAttempts.Should().Be(2);
	}

	[Fact]
	public void RecordFailedLogin_FiveAttempts_ShouldLockAccount()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");

		// Act
		for (int i = 0; i < 5; i++)
		{
			user.RecordFailedLogin();
		}

		// Assert
		user.Status.Should().Be(UserStatus.Locked);
		user.LockoutEndAt.Should().NotBeNull();
	}

	[Fact]
	public void RecordSuccessfulLogin_ShouldResetFailedAttempts()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");
		user.RecordFailedLogin();
		user.RecordFailedLogin();

		// Act
		user.RecordSuccessfulLogin();

		// Assert
		user.FailedLoginAttempts.Should().Be(0);
		user.LastLoginAt.Should().NotBeNull();
	}

	[Fact]
	public void UpdateRefreshToken_ShouldSetTokenAndExpiry()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");
		var expiry = DateTime.UtcNow.AddDays(7);

		// Act
		user.UpdateRefreshToken("new-refresh-token", expiry);

		// Assert
		user.RefreshToken.Should().Be("new-refresh-token");
		user.RefreshTokenExpiresAt.Should().Be(expiry);
	}

	[Fact]
	public void EnableTwoFactor_ShouldEnableTwoFactorAuth()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");

		// Act
		user.EnableTwoFactor(TwoFactorMethod.Totp, "secret-key");

		// Assert
		user.TwoFactorEnabled.Should().BeTrue();
		user.TwoFactorMethod.Should().Be(TwoFactorMethod.Totp);
		user.TotpSecretKey.Should().Be("secret-key");
	}

	[Fact]
	public void DisableTwoFactor_ShouldDisableTwoFactorAuth()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");
		user.EnableTwoFactor(TwoFactorMethod.Totp, "secret-key");

		// Act
		user.DisableTwoFactor();

		// Assert
		user.TwoFactorEnabled.Should().BeFalse();
		user.TotpSecretKey.Should().BeNull();
	}

	[Fact]
	public void Unlock_ShouldResetLockoutAndActivate()
	{
		// Arrange
		var user = User.Create(_tenantId, "testuser", "test@email.com", "hash", "John", "Doe");
		for (int i = 0; i < 5; i++) user.RecordFailedLogin();
		user.Status.Should().Be(UserStatus.Locked);

		// Act
		user.Unlock();

		// Assert
		user.Status.Should().Be(UserStatus.Active);
		user.FailedLoginAttempts.Should().Be(0);
		user.LockoutEndAt.Should().BeNull();
	}
}