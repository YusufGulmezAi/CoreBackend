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
	public TenantStatus Status { get; private set; }

	/// <summary>
	/// Abonelik başlangıç tarihi.
	/// </summary>
	public DateTime SubscriptionStartDate { get; private set; }

	/// <summary>
	/// Abonelik bitiş tarihi.
	/// </summary>
	public DateTime? SubscriptionEndDate { get; private set; }

	/// <summary>
	/// Maksimum yönetilebilecek alt organizasyon sayısı.
	/// </summary>
	public int MaxCompanyCount { get; private set; }

	/// <summary>
	/// Tenant ayarları (JSON formatında).
	/// </summary>
	public string? Settings { get; private set; }

	// EF Core için private constructor
	private Tenant() : base() { }

	private Tenant(
		Guid id,
		string name,
		string email,
		string? phone,
		int maxCompanyCount) : base(id)
	{
		Name = name;
		Email = email;
		Phone = phone;
		Status = TenantStatus.Trial;
		SubscriptionStartDate = DateTime.UtcNow;
		MaxCompanyCount = maxCompanyCount;
	}

	/// <summary>
	/// Yeni tenant oluşturur.
	/// </summary>
	public static Tenant Create(
		string name,
		string email,
		string? phone = null,
		int maxCompanyCount = 5)
	{
		return new Tenant(
			Guid.NewGuid(),
			name,
			email,
			phone,
			maxCompanyCount);
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
}