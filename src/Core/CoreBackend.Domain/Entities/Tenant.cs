using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// Tenant entity'si.
/// Sistemin ana müşterisi. Multi-tenant mimaride en üst seviye organizasyon.
/// Örnek: Mali müşavirlik, dershane, hastane grubu, franchise merkezi vb.
/// </summary>
public class Tenant : AuditableEntity<Guid>
{
	/// <summary>
	/// Tenant adı (Organizasyon/Firma/Kişi adı).
	/// </summary>
	public string Name { get; private set; } = null!;

	/// <summary>
	/// İletişim email adresi.
	/// </summary>
	public string Email { get; private set; } = null!;

	/// <summary>
	/// Telefon numarası.
	/// </summary>
	public string? Phone { get; private set; }

	/// <summary>
	/// Hesap durumu.
	/// </summary>
	public TenantStatus? Status { get; private set; }

	/// <summary>
	/// Abonelik başlangıç tarihi.
	/// </summary>
	public DateTime? SubscriptionStartDate { get; private set; }

	/// <summary>
	/// Abonelik bitiş tarihi.
	/// </summary>
	public DateTime? SubscriptionEndDate { get; private set; }

	/// <summary>
	/// Maksimum yönetilebilecek alt organizasyon sayısı.
	/// </summary>
	public int MaxCompanyCount { get; private set; }

	/// <summary>
	/// Alt organizasyonlar için kullanılacak subdomain. Örnek: tenant1.example.com, tenant2.example.com
	/// </summary>
	public string Subdomain { get; set; }

	/// <summary>
	/// İletişim emaili. Destek veya faturalandırma gibi konularda kullanılabilir.
	/// </summary>
	public string ContactEmail { get; set; }

	/// <summary>
	/// İletişim telefonu. Destek veya faturalandırma gibi konularda kullanılabilir.
	/// </summary>
	public string ContactPhone { get; set; }

	/// <summary>
	/// Tenant ayarları (JSON formatında).
	/// </summary>
	public string? Settings { get; private set; }

	/// <summary>
	/// Tenant bazlı session zaman aşımı süresi (dakika).
	/// null ise global ayar kullanılır.
	/// </summary>
	public int? SessionTimeoutMinutes { get; private set; }

	// 2FA Policy Alanları (mevcut alanlara ekle)
	public TwoFactorPolicy? TwoFactorPolicy { get; private set; }

	/// <summary>
	/// Geçerli kullanıcı veya bağlam için izin verilen iki faktörlü kimlik doğrulama yöntemlerinin listesini alır.
	/// </summary>
	/// <remarks>Koleksiyon, kullanıcının kimlik doğrulama için seçebileceği veya kullanabileceği tüm mevcut iki faktörlü yöntemleri içerir.
	/// Liste salt okunurdur ve hiçbir yönteme izin verilmiyorsa boş olacaktır.</remarks>
	public virtual ICollection<TwoFactorMethod> AllowedTwoFactorMethods { get; private set; } = new List<TwoFactorMethod>();

	/// <summary>
	/// Kullanıcı-Tenant ilişkileri.
	/// </summary>
	public virtual ICollection<User> Users { get; private set; } = new List<User>();

	/// <summary>
	/// Şirketler (alt organizasyonlar).
	/// </summary>
	public virtual ICollection<Company> Companies { get; private set; } = new List<Company>();

	/// <summary>
	/// Roller.
	/// </summary>
	public virtual ICollection<Role> Roles { get; private set; } = new List<Role>();

	/// <summary>
	/// Kullanıcı oturumları.
	/// </summary>
	public virtual ICollection<UserSession> UserSessions { get; private set; } = new List<UserSession>();

	// EF Core için private constructor
	private Tenant() : base()
	{
		// Set default values for non-nullable properties in the private parameterless constructor
		Subdomain = string.Empty;
		ContactEmail = string.Empty;
		ContactPhone = string.Empty;
	}

	private Tenant(
		Guid id,
		string name,
		string email,
		string? phone,
		int maxCompanyCount,
		int? sessionTimeoutMinutes) : base(id)
	{
		Name = name;
		Email = email;
		Phone = phone;
		Status = TenantStatus.Active;
		MaxCompanyCount = maxCompanyCount;
		SessionTimeoutMinutes = sessionTimeoutMinutes;
		SubscriptionStartDate = DateTime.UtcNow;
		Subdomain = name.ToLowerInvariant().Replace(" ", "-");
		ContactEmail = email;
		ContactPhone = phone ?? "";
	}

		/// <summary>
	/// Yeni tenant oluşturur.
	/// </summary>
	public static Tenant Create(
		string name,
		string email,
		string? phone = null,
		int maxCompanyCount = 5,
		int? sessionTimeoutMinutes = null)
	{
		return new Tenant(
			Guid.NewGuid(),
			name,
			email,
			phone,
			maxCompanyCount,
			sessionTimeoutMinutes);
	}


	/// <summary>
	/// Tenant bilgilerini günceller.
	/// </summary>
	public void Update(string name, string email, string? phone)
	{
		Name = name;
		Email = email;
		Phone = phone;
	}

	/// <summary>
	/// Tenant'ı aktif eder.
	/// </summary>
	public void Activate()
	{
		Status = TenantStatus.Active;
	}

	/// <summary>
	/// Tenant'ı pasif eder.
	/// </summary>
	public void Deactivate()
	{
		Status = TenantStatus.Inactive;
	}

	/// <summary>
	/// Tenant'ı askıya alır.
	/// </summary>
	public void Suspend()
	{
		Status = TenantStatus.Suspended;
	}

	/// <summary>
	/// Aboneliği yeniler.
	/// </summary>
	public void RenewSubscription(DateTime endDate, int? newMaxCompanyCount = null)
	{
		SubscriptionEndDate = endDate;

		if (newMaxCompanyCount.HasValue)
		{
			MaxCompanyCount = newMaxCompanyCount.Value;
		}

		if (Status == TenantStatus.Suspended || Status == TenantStatus.Trial)
		{
			Status = TenantStatus.Active;
		}
	}

	/// <summary>
	/// Ayarları günceller.
	/// </summary>
	public void UpdateSettings(string settings)
	{
		Settings = settings;
	}

	/// <summary>
	/// Session timeout süresini günceller.
	/// </summary>
	public void UpdateSessionTimeout(int? sessionTimeoutMinutes)
	{
		SessionTimeoutMinutes = sessionTimeoutMinutes;
	}

	// 2FA Policy Metodları
	/// <summary>
	/// 2FA politikasını ayarlar.
	/// </summary>
	public void SetTwoFactorPolicy(TwoFactorPolicy policy)
	{
		TwoFactorPolicy = policy;
	}

	/// <summary>
	/// İzin verilen 2FA metodlarını ayarlar.
	/// </summary>
	public void SetAllowedTwoFactorMethods(List<TwoFactorMethod> methods)
	{
		AllowedTwoFactorMethods = methods;
	}

}