using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Domain.Entities;

/// <summary>
/// Company entity'si.
/// Tenant'ın yönettiği alt organizasyon birimi.
/// Örnek: Şirket, dershane, şube, mağaza, klinik vb.
/// </summary>
public class Company : TenantAuditableEntity<Guid>
{
	/// <summary>
	/// Organizasyon adı.
	/// </summary>
	public string Name { get; private set; } = null!;

	/// <summary>
	/// Kısa kod (hızlı erişim için).
	/// </summary>
	public string Code { get; private set; } = null!;

	/// <summary>
	/// Vergi numarası (opsiyonel, sektöre göre).
	/// </summary>
	public string? TaxNumber { get; private set; }

	/// <summary>
	/// Vergi dairesi (opsiyonel).
	/// </summary>
	public string? TaxOffice { get; private set; }

	/// <summary>
	/// İletişim email adresi.
	/// </summary>
	public string? Email { get; private set; }

	/// <summary>
	/// Telefon numarası.
	/// </summary>
	public string? Phone { get; private set; }

	/// <summary>
	/// Adres bilgisi.
	/// </summary>
	public string? Address { get; private set; }

	/// <summary>
	/// Organizasyon durumu.
	/// </summary>
	public CompanyStatus Status { get; private set; }

	/// <summary>
	/// Organizasyon ayarları (JSON formatında).
	/// </summary>
	public string? Settings { get; private set; }

	// EF Core için private constructor
	private Company() : base() { }

	private Company(
		Guid id,
		Guid tenantId,
		string name,
		string code) : base(id, tenantId)
	{
		Name = name;
		Code = code;
		Status = CompanyStatus.Active;
	}

	/// <summary>
	/// Yeni organizasyon oluşturur.
	/// </summary>
	public static Company Create(
		Guid tenantId,
		string name,
		string code)
	{
		return new Company(
			Guid.NewGuid(),
			tenantId,
			name,
			code);
	}

	/// <summary>
	/// Temel bilgileri günceller.
	/// </summary>
	public void Update(string name, string code)
	{
		Name = name;
		Code = code;
	}

	/// <summary>
	/// İletişim bilgilerini günceller.
	/// </summary>
	public void UpdateContact(string? email, string? phone, string? address)
	{
		Email = email;
		Phone = phone;
		Address = address;
	}

	/// <summary>
	/// Vergi bilgilerini günceller.
	/// </summary>
	public void UpdateTaxInfo(string? taxNumber, string? taxOffice)
	{
		TaxNumber = taxNumber;
		TaxOffice = taxOffice;
	}

	/// <summary>
	/// Organizasyonu aktif eder.
	/// </summary>
	public void Activate()
	{
		Status = CompanyStatus.Active;
	}

	/// <summary>
	/// Organizasyonu pasif eder.
	/// </summary>
	public void Deactivate()
	{
		Status = CompanyStatus.Inactive;
	}

	/// <summary>
	/// Organizasyonu kapatır.
	/// </summary>
	public void Close()
	{
		Status = CompanyStatus.Closed;
	}

	/// <summary>
	/// Ayarları günceller.
	/// </summary>
	public void UpdateSettings(string settings)
	{
		Settings = settings;
	}

	/// <summary>
	/// Şirketi günceller.
	/// </summary>
	public void Update(
		string name,
		string? taxNumber,
		string? address,
		string? phone,
		string? email)
	{
		Name = name;
		TaxNumber = taxNumber;
		Address = address;
		Phone = phone;
		Email = email;
	}

	/// <summary>
	/// Şirketi siler (soft delete).
	/// </summary>
	public void Delete()
	{
		Status = CompanyStatus.Inactive;
		IsDeleted = true;
		DeletedAt = DateTime.UtcNow;
	}

}