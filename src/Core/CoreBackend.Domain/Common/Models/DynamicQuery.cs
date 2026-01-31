namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Dinamik sorgu modeli.
/// Frontend'den gelen tüm sorgu parametrelerini içerir.
/// </summary>
public class DynamicQuery
{
	/// <summary>
	/// Filtreler.
	/// </summary>
	public FilterGroup? Filter { get; set; }

	/// <summary>
	/// Sıralamalar (birden fazla alan için).
	/// </summary>
	public List<SortDescriptor> Sort { get; set; } = new();

	/// <summary>
	/// Include edilecek navigation property'ler.
	/// </summary>
	public List<string> Includes { get; set; } = new();

	/// <summary>
	/// Seçilecek alanlar (projection).
	/// Boş ise tüm alanlar seçilir.
	/// </summary>
	public List<string> Select { get; set; } = new();

	/// <summary>
	/// Gruplama alanları.
	/// </summary>
	public List<string> GroupBy { get; set; } = new();

	/// <summary>
	/// Global filtreyi devre dışı bırak (Soft Delete, Tenant).
	/// Sadece admin işlemleri için.
	/// </summary>
	public bool IgnoreQueryFilters { get; set; } = false;

	/// <summary>
	/// AsNoTracking kullan (read-only sorgular için performans).
	/// </summary>
	public bool AsNoTracking { get; set; } = true;
}