namespace CoreBackend.Contracts.Common;


/// <summary>
/// Sayfalı yanıt modeli.
/// Frontend'e dönecek sayfalama bilgileri.
/// </summary>
/// <typeparam name="T">Veri tipi</typeparam>
public class PagedResponse<T>
{
	/// <summary>
	/// Veri listesi.
	/// </summary>
	public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

	/// <summary>
	/// Mevcut sayfa numarası.
	/// </summary>
	public int PageNumber { get; set; }

	/// <summary>
	/// Sayfa başına kayıt sayısı.
	/// </summary>
	public int PageSize { get; set; }

	/// <summary>
	/// Toplam kayıt sayısı.
	/// </summary>
	public int TotalCount { get; set; }

	/// <summary>
	/// Toplam sayfa sayısı.
	/// </summary>
	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

	/// <summary>
	/// Önceki sayfa var mı?
	/// </summary>
	public bool HasPreviousPage => PageNumber > 1;

	/// <summary>
	/// Sonraki sayfa var mı?
	/// </summary>
	public bool HasNextPage => PageNumber < TotalPages;

	/// <summary>
	/// İlk sayfa mı?
	/// </summary>
	public bool IsFirstPage => PageNumber == 1;

	/// <summary>
	/// Son sayfa mı?
	/// </summary>
	public bool IsLastPage => PageNumber == TotalPages;

	/// <summary>
	/// İlk kaydın sırası (1'den başlar).
	/// </summary>
	public int FirstItemIndex => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;

	/// <summary>
	/// Son kaydın sırası.
	/// </summary>
	public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCount);

	/// <summary>
	/// Boş response oluşturur.
	/// </summary>
	public static PagedResponse<T> Empty(int pageNumber = 1, int pageSize = 10)
	{
		return new PagedResponse<T>
		{
			Items = Array.Empty<T>(),
			PageNumber = pageNumber,
			PageSize = pageSize,
			TotalCount = 0
		};
	}

	/// <summary>
	/// Yeni response oluşturur.
	/// </summary>
	public static PagedResponse<T> Create(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
	{
		return new PagedResponse<T>
		{
			Items = items,
			PageNumber = pageNumber,
			PageSize = pageSize,
			TotalCount = totalCount
		};
	}
}