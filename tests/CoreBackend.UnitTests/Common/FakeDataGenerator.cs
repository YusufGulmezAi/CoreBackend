using Bogus;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;

namespace CoreBackend.UnitTests.Common;

/// <summary>
/// Test verisi oluşturucu (Factory Pattern).
/// 
/// NEDEN BU SINIF VAR?
/// -------------------
/// 1. Gerçekçi test verisi: "asdfgh" yerine gerçek isimler, email'ler üretir
/// 2. Tek kaynak: Tüm testler aynı şekilde veri oluşturur
/// 3. Kolay özelleştirme: Sadece gerekli alanları override edebilirsin
/// 4. Türkçe desteği: Faker("tr") ile Türkçe veri üretir
/// 
/// BOGUS KÜTÜPHANESİ:
/// ------------------
/// - Faker.Name.FirstName() -> "Ahmet", "Mehmet", "Ayşe"
/// - Faker.Internet.Email() -> "ahmet.yilmaz@gmail.com"
/// - Faker.Company.CompanyName() -> "ABC Teknoloji A.Ş."
/// - Faker.Phone.PhoneNumber() -> "+90 555 123 45 67"
/// 
/// KULLANIM:
/// ---------
/// var tenant = FakeDataGenerator.CreateTenant();                    // Varsayılan
/// var tenant = FakeDataGenerator.CreateTenant(name: "Özel İsim");   // Özelleştirilmiş
/// var tenant = FakeDataGenerator.CreateTenant(status: TenantStatus.Inactive);
/// </summary>
public static class FakeDataGenerator
{
	/// <summary>
	/// Türkçe lokalizasyon ile Faker instance.
	/// "tr" parametresi Türkçe isim, adres vb. üretir.
	/// </summary>
	private static readonly Faker Faker = new("tr");

	/// <summary>
	/// Test için Tenant entity'si oluşturur.
	/// 
	/// NEDEN FACTORY METHOD?
	/// ---------------------
	/// - Tenant.Create() metodunu doğrudan çağırmak yerine bu metodu kullanıyoruz
	/// - Böylece test verisi oluşturma mantığı tek yerde
	/// - Tenant.Create() parametreleri değişirse sadece burayı güncelliyoruz
	/// 
	/// PARAMETRELER:
	/// -------------
	/// - name: null ise rastgele şirket ismi üretilir
	/// - email: null ise rastgele email üretilir  
	/// - status: Varsayılan Active, istersen Inactive/Suspended yapabilirsin
	/// </summary>
	public static Tenant CreateTenant(
		string? name = null,
		string? email = null,
		TenantStatus status = TenantStatus.Active)
	{
		// Faker ile rastgele ama gerçekçi veri üret
		var tenant = Tenant.Create(
			name ?? Faker.Company.CompanyName(),      // "ABC Teknoloji A.Ş."
			email ?? Faker.Internet.Email(),          // "info@abc.com.tr"
			Faker.Phone.PhoneNumber(),                // "+90 555 123 4567"
			maxCompanyCount: 10,
			sessionTimeoutMinutes: 60);

		// İstenen duruma göre tenant'ı ayarla
		if (status != TenantStatus.Active)
		{
			switch (status)
			{
				case TenantStatus.Inactive:
					tenant.Deactivate();
					break;
				case TenantStatus.Suspended:
					tenant.Suspend();
					break;
			}
		}

		return tenant;
	}

	/// <summary>
	/// Test için User entity'si oluşturur.
	/// 
	/// NOT: passwordHash gerçek bir hash değil, test için sabit değer.
	/// Çünkü unit testlerde şifre doğrulama yapmıyoruz.
	/// </summary>
	public static User CreateUser(
		Guid? tenantId = null,
		string? email = null,
		string? username = null)
	{
		return User.Create(
			tenantId ?? Guid.NewGuid(),               // Rastgele veya verilen TenantId
			username ?? Faker.Internet.UserName(),    // "ahmet_yilmaz"
			email ?? Faker.Internet.Email(),          // "ahmet@email.com"
			"hashedPassword123",                      // Test için sabit (gerçek hash değil)
			Faker.Name.FirstName(),                   // "Ahmet"
			Faker.Name.LastName(),                    // "Yılmaz"
			Faker.Phone.PhoneNumber());               // "+90 555..."
	}

	/// <summary>
	/// Test için Company entity'si oluşturur.
	/// </summary>
	public static Company CreateCompany(
		Guid? tenantId = null,
		string? name = null,
		string? code = null)
	{
		return Company.Create(
			tenantId ?? Guid.NewGuid(),
			name ?? Faker.Company.CompanyName(),
			code ?? Faker.Random.AlphaNumeric(6).ToUpper(),  // "ABC123"
			Faker.Finance.Account(),                          // Vergi No benzeri
			Faker.Address.FullAddress(),                      // Tam adres
			Faker.Phone.PhoneNumber(),
			Faker.Internet.Email());
	}

	/// <summary>
	/// Test için Role entity'si oluşturur.
	/// 
	/// PARAMETRELER:
	/// - tenantId: null ise rastgele GUID
	/// - name: null ise rastgele job title
	/// - level: Varsayılan Tenant seviyesi
	/// - isSystemRole: Varsayılan false
	/// </summary>
	public static Role CreateRole(
		Guid? tenantId = null,
		string? name = null,
		string? code = null,
		RoleLevel level = RoleLevel.Tenant,
		bool isSystemRole = false,
		int? sessionTimeoutMinutes = 60)
	{
		return Role.Create(
			tenantId ?? Guid.NewGuid(),
			name ?? Faker.Name.JobTitle(),
			code ?? Faker.Random.AlphaNumeric(10).ToUpper(),
			level,
			Faker.Lorem.Sentence(),
			isSystemRole,
			sessionTimeoutMinutes);
	}

	/// <summary>
	/// Test için Permission listesi oluşturur.
	/// </summary>
	public static List<Permission> CreatePermissions(int count = 5)
	{
		var permissions = new List<Permission>();
		for (int i = 0; i < count; i++)
		{
			permissions.Add(Permission.Create(
				Faker.Lorem.Word(),                              // "read"
				$"{Faker.Lorem.Word()}.{Faker.Lorem.Word()}",    // "users.read"
				Faker.Lorem.Word(),                              // "User Management"
				Faker.Lorem.Sentence()));                        // Açıklama
		}
		return permissions;
	}
}