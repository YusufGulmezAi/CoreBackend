namespace CoreBackend.Domain.Errors;

/// <summary>
/// Uygulama genelinde kullanılan hata kodları.
/// Kategorilere ayrılmış sabit değerler.
/// </summary>
public static class ErrorCodes
{
	/// <summary>
	/// Genel hatalar
	/// </summary>
	public static class General
	{
		/// <summary>
		/// Beklenmeyen bir hata oluştu.
		/// </summary>
		public const string UnexpectedError = "GENERAL_UNEXPECTED_ERROR";

		/// <summary>
		/// Validasyon hatası. Girilen veriler geçersiz.
		/// </summary>
		public const string ValidationError = "GENERAL_VALIDATION_ERROR";

		/// <summary>
		/// Aranan kayıt bulunamadı.
		/// </summary>
		public const string NotFound = "GENERAL_NOT_FOUND";

		/// <summary>
		/// Null değer sağlandı, ancak değer gerekli.
		/// </summary>
		public const string NullValue = "GENERAL_NULL_VALUE";

		/// <summary>
		/// Geçersiz işlem. Bu işlem şu anda yapılamaz.
		/// </summary>
		public const string InvalidOperation = "GENERAL_INVALID_OPERATION";

		/// <summary>
		/// Çakışma. Kayıt zaten mevcut veya eş zamanlı güncelleme hatası.
		/// </summary>
		public const string Conflict = "GENERAL_CONFLICT";
	}

	/// <summary>
	/// Authentication / Authorization hataları
	/// </summary>
	public static class Auth
	{
		/// <summary>
		/// Geçersiz kullanıcı adı veya şifre.
		/// </summary>
		public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";

		/// <summary>
		/// Kullanıcı bulunamadı.
		/// </summary>
		public const string UserNotFound = "AUTH_USER_NOT_FOUND";

		/// <summary>
		/// Kullanıcı hesabı kilitli. Çok fazla başarısız giriş denemesi.
		/// </summary>
		public const string UserLocked = "AUTH_USER_LOCKED";

		/// <summary>
		/// Kullanıcı hesabı pasif durumda.
		/// </summary>
		public const string UserInactive = "AUTH_USER_INACTIVE";

		/// <summary>
		/// Token süresi dolmuş. Yeniden giriş yapılmalı.
		/// </summary>
		public const string TokenExpired = "AUTH_TOKEN_EXPIRED";

		/// <summary>
		/// Token geçersiz veya manipüle edilmiş.
		/// </summary>
		public const string TokenInvalid = "AUTH_TOKEN_INVALID";

		/// <summary>
		/// Yetkisiz erişim. Giriş yapılmamış.
		/// </summary>
		public const string Unauthorized = "AUTH_UNAUTHORIZED_ACCESS";

		/// <summary>
		/// Yasaklı erişim. Bu kaynağa erişim izniniz yok.
		/// </summary>
		public const string ForbiddenAccess = "AUTH_FORBIDDEN_ACCESS";

		/// <summary>
		/// Session süresi dolmuş.
		/// </summary>
		public const string SessionExpired = "AUTH_SESSION_EXPIRED";

		/// <summary>
		/// Refresh token geçersiz.
		/// </summary>
		public const string RefreshTokenInvalid = "AUTH_REFRESH_TOKEN_INVALID";

		/// <summary>
		/// Refresh token süresi dolmuş.
		/// </summary>
		public const string RefreshTokenExpired = "AUTH_REFRESH_TOKEN_EXPIRED";
	}

	/// <summary>
	/// Tenant (Mali Müşavir/Muhasebeci) hataları
	/// </summary>
	public static class Tenant
	{
		/// <summary>
		/// Tenant bulunamadı.
		/// </summary>
		public const string NotFound = "TENANT_NOT_FOUND";

		/// <summary>
		/// Tenant hesabı pasif durumda.
		/// </summary>
		public const string Inactive = "TENANT_INACTIVE";

		/// <summary>
		/// Tenant hesabı askıya alınmış. Ödeme veya ihlal nedeniyle.
		/// </summary>
		public const string Suspended = "TENANT_SUSPENDED";

		/// <summary>
		/// Bu tenant'a erişim izniniz yok.
		/// </summary>
		public const string InvalidAccess = "TENANT_INVALID_ACCESS";

		/// <summary>
		/// Bu tenant zaten kayıtlı.
		/// </summary>
		public const string AlreadyExists = "TENANT_ALREADY_EXISTS";
	}

	/// <summary>
	/// Company (Şirket) hataları
	/// </summary>
	public static class Company
	{
		/// <summary>
		/// Şirket bulunamadı.
		/// </summary>
		public const string NotFound = "COMPANY_NOT_FOUND";

		/// <summary>
		/// Şirket pasif durumda.
		/// </summary>
		public const string Inactive = "COMPANY_INACTIVE";

		/// <summary>
		/// Bu şirket zaten kayıtlı.
		/// </summary>
		public const string AlreadyExists = "COMPANY_ALREADY_EXISTS";

		/// <summary>
		/// Geçersiz vergi numarası formatı.
		/// </summary>
		public const string InvalidTaxNumber = "COMPANY_INVALID_TAX_NUMBER";

		/// <summary>
		/// Bu şirkete erişim izniniz yok.
		/// </summary>
		public const string InvalidAccess = "COMPANY_INVALID_ACCESS";
	}

	/// <summary>
	/// Veritabanı hataları
	/// </summary>
	public static class Database
	{
		/// <summary>
		/// Veritabanı bağlantısı kurulamadı.
		/// </summary>
		public const string ConnectionFailed = "DB_CONNECTION_FAILED";

		/// <summary>
		/// Sorgu çalıştırılamadı.
		/// </summary>
		public const string QueryFailed = "DB_QUERY_FAILED";

		/// <summary>
		/// Transaction başarısız oldu. İşlemler geri alındı.
		/// </summary>
		public const string TransactionFailed = "DB_TRANSACTION_FAILED";

		/// <summary>
		/// Eş zamanlılık çakışması. Kayıt başka biri tarafından güncellendi.
		/// </summary>
		public const string ConcurrencyConflict = "DB_CONCURRENCY_CONFLICT";
	}

	public static class User
	{
		public const string NotFound = "USER_NOT_FOUND";
		public const string AlreadyExists = "USER_ALREADY_EXISTS";
		public const string EmailAlreadyExists = "USER_EMAIL_ALREADY_EXISTS";
		public const string CannotDeleteSelf = "USER_CANNOT_DELETE_SELF";
	}

	public static class Session
	{
		public const string NotFound = "SESSION_NOT_FOUND";
		public const string Expired = "SESSION_EXPIRED";
		public const string Revoked = "SESSION_REVOKED";
		public const string InvalidIp = "SESSION_INVALID_IP";
		public const string InvalidUserAgent = "SESSION_INVALID_USER_AGENT";
	}

	public static class TwoFactor
	{
		public const string NotEnabled = "2FA_NOT_ENABLED";
		public const string AlreadyEnabled = "2FA_ALREADY_ENABLED";
		public const string InvalidCode = "2FA_INVALID_CODE";
		public const string CodeExpired = "2FA_CODE_EXPIRED";
		public const string MaxAttemptsExceeded = "2FA_MAX_ATTEMPTS_EXCEEDED";
		public const string MethodNotAllowed = "2FA_METHOD_NOT_ALLOWED";
		public const string Required = "2FA_REQUIRED";
		public const string InvalidRecoveryCode = "2FA_INVALID_RECOVERY_CODE";
		public const string NoRecoveryCodes = "2FA_NO_RECOVERY_CODES";
		public const string SetupRequired = "2FA_SETUP_REQUIRED";
	}

	public static class Sms
	{
		public const string SendFailed = "SMS_SEND_FAILED";
		public const string InvalidPhoneNumber = "SMS_INVALID_PHONE_NUMBER";
		public const string QuotaExceeded = "SMS_QUOTA_EXCEEDED";
	}

	public static class Email
	{
		public const string SendFailed = "EMAIL_SEND_FAILED";
	}

}