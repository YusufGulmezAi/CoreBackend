using AutoFixture;
using AutoFixture.Xunit2;
using Moq;

namespace CoreBackend.UnitTests.Common;

/// <summary>
/// Tüm unit testler için temel sınıf.
/// 
/// NEDEN BU SINIF VAR?
/// -------------------
/// 1. Kod tekrarını önler - Her test sınıfında aynı setup kodunu yazmamak için
/// 2. Tutarlılık sağlar - Tüm testler aynı altyapıyı kullanır
/// 3. Test verisi oluşturmayı kolaylaştırır - AutoFixture ile otomatik veri üretimi
/// 
/// NE YAPAR?
/// ---------
/// - AutoFixture: Rastgele test verisi oluşturur (string, int, complex objects)
/// - Mock<T>: Bağımlılıkları taklit eder (database, servisler vs.)
/// 
/// KULLANIM:
/// ---------
/// public class MyTests : TestBase
/// {
///     [Fact]
///     public void MyTest()
///     {
///         var randomString = Fixture.Create<string>();
///         var mockService = CreateMock<IMyService>();
///     }
/// }
/// </summary>
public abstract class TestBase
{
	/// <summary>
	/// AutoFixture instance.
	/// Rastgele ama geçerli test verisi üretir.
	/// 
	/// ÖRNEK:
	/// var user = Fixture.Create<User>(); // Tüm property'leri dolu User nesnesi
	/// var emails = Fixture.CreateMany<string>(5); // 5 adet rastgele string
	/// </summary>
	protected readonly IFixture Fixture;

	protected TestBase()
	{
		Fixture = new Fixture();

		// Circular reference (döngüsel referans) sorununu çöz
		// Örnek: User -> Tenant -> Users -> User... gibi sonsuz döngüleri engeller
		Fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
			.ToList()
			.ForEach(b => Fixture.Behaviors.Remove(b));
		Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
	}

	/// <summary>
	/// Mock nesnesi oluşturur.
	/// 
	/// NEDEN MOCK KULLANIYORUZ?
	/// ------------------------
	/// Unit testlerde sadece test edilen kodu test etmek istiyoruz.
	/// Veritabanı, HTTP çağrıları, dosya sistemi gibi dış bağımlılıkları
	/// gerçek değil, sahte (mock) versiyonlarıyla değiştiriyoruz.
	/// 
	/// ÖRNEK:
	/// var mockRepo = CreateMock<IUserRepository>();
	/// mockRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
	///         .ReturnsAsync(new User());
	/// </summary>
	protected Mock<T> CreateMock<T>() where T : class => new();
}