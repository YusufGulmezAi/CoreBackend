using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// User entity'si.
/// Sisteme giriş yapan kullanıcıları temsil eder.
/// Her kullanıcı bir Tenant'a bağlıdır.
/// </summary>
public class User : TenantAuditableEntity<Guid>
{
	/// <summary>
	/// Kullanıcı adı (giriş için).
	/// </summary>
	public string Username { get; private set; } = null!;

	/// <summary>
	/// Email adresi.
	/// </summary>
	public string Email { get; private set; } = null!;

	/// <summary>
	/// Şifre hash'i.
	/// </summary>
	public string PasswordHash { get; private set; } = null!;

	/// <summary>
	/// Ad.
	/// </summary>
	public string FirstName { get; private set; } = null!;

	/// <summary>
	/// Soyad.
	/// </summary>
	public string LastName { get; private set; } = null!;

	/// <summary>
	/// Telefon numarası.
	/// </summary>
	public string? Phone { get; private set; }

	/// <summary>
	/// Hesap durumu.
	/// </summary>
	public UserStatus Status { get; private set; }

	/// <summary>
	/// Email doğrulandı mı?
	/// </summary>
	public bool EmailConfirmed { get; private set; }

	/// <summary>
	/// Son giriş tarihi.
	/// </summary>
	public DateTime? LastLoginAt { get; private set; }

	/// <summary>
	/// Başarısız giriş denemesi sayısı.
	/// </summary>
	public int FailedLoginAttempts { get; private set; }

	/// <summary>
	/// Hesap kilitleme bitiş tarihi.
	/// </summary>
	public DateTime? LockoutEndAt { get; private set; }

	/// <summary>
	/// Refresh token (JWT için).
	/// </summary>
	public string? RefreshToken { get; private set; }

	/// <summary>
	/// Refresh token geçerlilik tarihi.
	/// </summary>
	public DateTime? RefreshTokenExpiresAt { get; private set; }

	// 2FA Alanları (mevcut alanlara ekle)
	public bool TwoFactorEnabled { get; private set; }
	public TwoFactorMethod TwoFactorMethod { get; private set; }
	public string? TotpSecretKey { get; private set; }
	public string? RecoveryCodes { get; private set; } // JSON array
	public int RecoveryCodeCount { get; private set; }

	/// <summary>
	/// Kullanıcı ayarları (JSON formatında).
	/// </summary>
	public string? Settings { get; private set; }

	/// <summary>
	/// Ad Soyad birleşik.
	/// </summary>
	public string FullName => $"{FirstName} {LastName}";

	// EF Core için private constructor
	private User() : base() { }

	private User(
		Guid id,
		Guid tenantId,
		string username,
		string email,
		string passwordHash,
		string firstName,
		string lastName) : base(id, tenantId)
	{
		Username = username;
		Email = email;
		PasswordHash = passwordHash;
		FirstName = firstName;
		LastName = lastName;
		Status = UserStatus.PendingVerification;
		EmailConfirmed = false;
		FailedLoginAttempts = 0;
	}

	/// <summary>
	/// Yeni kullanıcı oluşturur.
	/// </summary>
	public static User Create(
		Guid tenantId,
		string username,
		string email,
		string passwordHash,
		string firstName,
		string lastName,
		string? phone = null)
	{
		return new User(
			Guid.NewGuid(),
			tenantId,
			username,
			email,
			passwordHash,
			firstName,
			lastName);
	}

	/// <summary>
	/// Profil bilgilerini günceller.
	/// </summary>
	public void UpdateProfile(string firstName, string lastName, string? phone)
	{
		FirstName = firstName;
		LastName = lastName;
		Phone = phone;
	}

	/// <summary>
	/// Email adresini günceller.
	/// </summary>
	public void UpdateEmail(string email)
	{
		Email = email;
		EmailConfirmed = false;
	}

	/// <summary>
	/// Şifreyi günceller.
	/// </summary>
	public void UpdatePassword(string passwordHash)
	{
		PasswordHash = passwordHash;
	}

	/// <summary>
	/// Email'i doğrular.
	/// </summary>
	public void ConfirmEmail()
	{
		EmailConfirmed = true;

		if (Status == UserStatus.PendingVerification)
		{
			Status = UserStatus.Active;
		}
	}

	/// <summary>
	/// Başarılı giriş kaydeder.
	/// </summary>
	public void RecordSuccessfulLogin()
	{
		LastLoginAt = DateTime.UtcNow;
		FailedLoginAttempts = 0;
		LockoutEndAt = null;
	}

	/// <summary>
	/// Başarısız giriş kaydeder.
	/// </summary>
	public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
	{
		FailedLoginAttempts++;

		if (FailedLoginAttempts >= maxAttempts)
		{
			Status = UserStatus.Locked;
			LockoutEndAt = DateTime.UtcNow.AddMinutes(lockoutMinutes);
		}
	}

	/// <summary>
	/// Hesap kilidini kaldırır.
	/// </summary>
	public void Unlock()
	{
		Status = UserStatus.Active;
		FailedLoginAttempts = 0;
		LockoutEndAt = null;
	}

	/// <summary>
	/// Refresh token'ı günceller.
	/// </summary>
	public void UpdateRefreshToken(string refreshToken, DateTime expiresAt)
	{
		RefreshToken = refreshToken;
		RefreshTokenExpiresAt = expiresAt;
	}

	/// <summary>
	/// Refresh token'ı temizler.
	/// </summary>
	public void RevokeRefreshToken()
	{
		RefreshToken = null;
		RefreshTokenExpiresAt = null;
	}

	/// <summary>
	/// Hesabı aktif eder.
	/// </summary>
	public void Activate()
	{
		Status = UserStatus.Active;
	}

	/// <summary>
	/// Hesabı pasif eder.
	/// </summary>
	public void Deactivate()
	{
		Status = UserStatus.Inactive;
	}

	/// <summary>
	/// Ayarları günceller.
	/// </summary>
	public void UpdateSettings(string settings)
	{
		Settings = settings;
	}

	/// <summary>
	/// Şifre değiştirir.
	/// </summary>
	public void ChangePassword(string newPasswordHash)
	{
		PasswordHash = newPasswordHash;
	}

	/// <summary>
	/// Kullanıcıyı siler (soft delete).
	/// </summary>
	public void Delete()
	{
		Status = UserStatus.Inactive;
	}

	/// <summary>
	/// Recovery kodlarını ayarlar.
	/// </summary>
	public void SetRecoveryCodes(string recoveryCodesJson, int count)
	{
		RecoveryCodes = recoveryCodesJson;
		RecoveryCodeCount = count;
	}

	// 2FA Metodları
	/// <summary>
	/// 2FA'yı aktifleştirir.
	/// </summary>
	public void EnableTwoFactor(TwoFactorMethod method, string? totpSecretKey = null)
	{
		TwoFactorEnabled = true;
		TwoFactorMethod = method;

		if (method == TwoFactorMethod.Totp && !string.IsNullOrEmpty(totpSecretKey))
		{
			TotpSecretKey = totpSecretKey;
		}
	}

	/// <summary>
	/// 2FA'yı devre dışı bırakır.
	/// </summary>
	public void DisableTwoFactor()
	{
		TwoFactorEnabled = false;
		TwoFactorMethod = TwoFactorMethod.None;
		TotpSecretKey = null;
		RecoveryCodes = null;
		RecoveryCodeCount = 0;
	}

	/// <summary>
	/// Recovery kod sayısını azaltır.
	/// </summary>
	public void UseRecoveryCode()
	{
		if (RecoveryCodeCount > 0)
		{
			RecoveryCodeCount--;
		}
	}

}