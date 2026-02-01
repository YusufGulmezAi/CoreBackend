namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Repository sorgu seçenekleri.
/// Sayfalama, arama, filtreleme ve sıralama bilgilerini taşır.
/// </summary>
public class QueryOptions
{
	/// <summary>
	/// Sayfa numarası (1'den başlar).
	/// </summary>
	public int PageNumber { get; set; } = 1;

	/// <summary>
	/// Sayfa başına kayıt sayısı.
	/// </summary>
	public int PageSize { get; set; } = 10;

	/// <summary>
	/// Arama metni.
	/// </summary>
	public string? SearchText { get; set; }

	/// <summary>
	/// Arama yapılacak alanlar.
	/// </summary>
	public List<string>? SearchFields { get; set; }

	/// <summary>
	/// Dinamik sorgu (filtre ve sıralama).
	/// </summary>
	public DynamicQuery? Query { get; set; }

	/// <summary>
	/// Varsayılan QueryOptions oluşturur.
	/// </summary>
	public static QueryOptions Default => new();

}