namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Sayfalı istek modeli.
/// Frontend'den gelen sayfalama parametreleri.
/// </summary>
public class PagedRequest
{
	private int _pageNumber = 1;
	private int _pageSize = 10;

	/// <summary>
	/// Sayfa numarası (1'den başlar).
	/// </summary>
	public int PageNumber
	{
		get => _pageNumber;
		set => _pageNumber = value < 1 ? 1 : value;
	}

	/// <summary>
	/// Sayfa başına kayıt sayısı.
	/// </summary>
	public int PageSize
	{
		get => _pageSize;
		set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
	}

	/// <summary>
	/// Maksimum sayfa boyutu (aşırı yüklenmeyi önlemek için).
	/// </summary>
	public static int MaxPageSize { get; set; } = 100;

	/// <summary>
	/// Dinamik sorgu parametreleri.
	/// </summary>
	public DynamicQuery? Query { get; set; }

	/// <summary>
	/// Hızlı arama metni (birden fazla alanda arama).
	/// </summary>
	public string? SearchText { get; set; }

	/// <summary>
	/// Hızlı arama yapılacak alanlar.
	/// </summary>
	public List<string> SearchFields { get; set; } = new();

	/// <summary>
	/// Atlanacak kayıt sayısını hesaplar.
	/// </summary>
	public int Skip => (PageNumber - 1) * PageSize;
}