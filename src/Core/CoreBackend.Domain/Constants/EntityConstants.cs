namespace CoreBackend.Domain.Constants;

/// <summary>
/// Entity property sabitleri.
/// EF Core Configuration ve FluentValidation'da kullanılır.
/// </summary>
public static class EntityConstants
{
	/// <summary>
	/// Tenant entity sabitleri.
	/// </summary>
	public static class Tenant
	{
		public const int NameMinLength = 2;
		public const int NameMaxLength = 200;
		public const int EmailMaxLength = 256;
		public const int PhoneMaxLength = 20;
		public const int MaxCompanyCountDefault = 5;
	}

	/// <summary>
	/// Company entity sabitleri.
	/// </summary>
	public static class Company
	{
		public const int NameMinLength = 2;
		public const int NameMaxLength = 200;
		public const int CodeMinLength = 2;
		public const int CodeMaxLength = 50;
		public const int TaxNumberMaxLength = 20;
		public const int TaxOfficeMaxLength = 100;
		public const int EmailMaxLength = 256;
		public const int PhoneMaxLength = 20;
		public const int AddressMaxLength = 500;
	}

	/// <summary>
	/// User entity sabitleri.
	/// </summary>
	public static class User
	{
		public const int UsernameMinLength = 3;
		public const int UsernameMaxLength = 100;
		public const int EmailMaxLength = 256;
		public const int PasswordHashMaxLength = 500;
		public const int FirstNameMinLength = 2;
		public const int FirstNameMaxLength = 100;
		public const int LastNameMinLength = 2;
		public const int LastNameMaxLength = 100;
		public const int PhoneMaxLength = 20;
		public const int RefreshTokenMaxLength = 500;
		public const int MaxFailedLoginAttempts = 5;
		public const int LockoutMinutes = 15;
	}

	/// <summary>
	/// Role entity sabitleri.
	/// </summary>
	public static class Role
	{
		public const int NameMinLength = 2;
		public const int NameMaxLength = 100;
		public const int CodeMinLength = 2;
		public const int CodeMaxLength = 50;
		public const int DescriptionMaxLength = 500;
	}

	/// <summary>
	/// Permission entity sabitleri.
	/// </summary>
	public static class Permission
	{
		public const int NameMinLength = 2;
		public const int NameMaxLength = 100;
		public const int CodeMinLength = 2;
		public const int CodeMaxLength = 100;
		public const int DescriptionMaxLength = 500;
		public const int GroupMinLength = 2;
		public const int GroupMaxLength = 100;
	}

	/// <summary>
	/// UserSession entity sabitleri.
	/// </summary>
	public static class UserSession
	{
		public const int SessionIdMaxLength = 100;
		public const int RefreshTokenMaxLength = 500;
		public const int IpAddressMaxLength = 50;
		public const int UserAgentMaxLength = 500;
		public const int GeoLocationMaxLength = 500;
		public const int BrowserNameMaxLength = 100;
		public const int OperatingSystemMaxLength = 100;
		public const int DeviceTypeMaxLength = 50;
		public const int RevokedReasonMaxLength = 500;
	}

	/// <summary>
	/// Ortak sabitler.
	/// </summary>
	public static class Common
	{
		public const int SettingsMaxLength = 4000;
		public const int TextMaxLength = -1; // nvarchar(max) / text
	}

	public static class SessionHistory
	{
		public const int SessionIdMaxLength = 64;
		public const int IpAddressMaxLength = 45;
		public const int UserAgentMaxLength = 500;
		public const int BrowserNameMaxLength = 100;
		public const int OperatingSystemMaxLength = 100;
		public const int DeviceTypeMaxLength = 50;
		public const int GeoLocationMaxLength = 200;
		public const int CountryMaxLength = 100;
		public const int CityMaxLength = 100;
		public const int RevokeReasonMaxLength = 500;
		public const int AdditionalDataMaxLength = 4000;
	}

	public static class TwoFactor
	{
		public const int CodeLength = 6;
		public const int TotpSecretKeyMaxLength = 100;
		public const int RecoveryCodesMaxLength = 2000;
		public const int RecoveryCodeCount = 10;
		public const int CodeExpirationMinutes = 5;
		public const int MaxAttempts = 5;
	}



}