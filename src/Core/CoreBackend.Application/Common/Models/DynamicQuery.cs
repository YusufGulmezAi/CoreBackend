namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Dinamik sorgu modeli.
/// Filtre ve sıralama bilgilerini taşır.
/// </summary>
public class DynamicQuery
{
	/// <summary>
	/// Sorguların döndürülen varlıklardaki değişiklikleri izlemeden yürütülüp yürütülmeyeceğini belirten bir değer alır veya ayarlar.
	/// </summary>
	/// <remarks> <see langword="true"/> olarak ayarlandığında, bağlam sorgular tarafından döndürülen varlıklardaki değişiklikleri izlemez; bu da salt okunur işlemler için performansı artırabilir.
	/// Sorgulanan varlıkları değiştirmeyi ve kaydetmeyi düşünüyorsanız<see langword="false"/> olarak ayarlayın.
	public bool AsNoTracking { get; set; } = true;

	/// <summary>
	/// Filtreler (AND ile birleştirilir).
	/// </summary>
	public List<FilterDescriptor>? Filters { get; set; }

	/// <summary>
	/// Filtre grupları (karmaşık AND/OR sorguları için).
	/// </summary>
	public List<FilterGroup>? FilterGroups { get; set; }

	/// <summary>
	/// Sıralama bilgileri.
	/// </summary>
	public List<SortDescriptor>? Sort { get; set; }

	/// <summary>
	/// Sorgu sonuçlarına dahil edilecek ilgili varlık adlarının listesini alır veya ayarlar.
	/// </summary>
	/// <remarks>Bu özelliği, sorgunun bir parçası olarak önceden yüklenmesi gereken gezinme özelliklerini veya ilgili verileri belirtmek için kullanın.
	///	Tam biçim ve desteklenen değerler, temel veri erişim uygulamasına bağlıdır.</remarks>
	public List<string>? Includes { get; set; }

	/// <summary>
	/// Filtre var mı?
	/// </summary>
	public bool HasFilters => (Filters?.Any() == true) || (FilterGroups?.Any() == true);

	/// <summary>
	/// Sıralama var mı?
	/// </summary>
	public bool HasSort => Sort?.Any() == true;


}