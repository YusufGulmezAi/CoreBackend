namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Repository sorgu sonucu.
/// Sayfalanmış veri ve metadata içerir.
/// </summary>
/// <typeparam name="T">Veri tipi</typeparam>
public class QueryResult<T>
{
	/// <summary>
	/// Sayfa verileri.
	/// </summary>
	public List<T> Items { get; set; } = new();

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
	public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

	/// <summary>
	/// Önceki sayfa var mı?
	/// </summary>
	public bool HasPreviousPage => PageNumber > 1;

	/// <summary>
	/// Sonraki sayfa var mı?
	/// </summary>
	public bool HasNextPage => PageNumber < TotalPages;

	public QueryResult() { }

	public QueryResult(List<T> items, int pageNumber, int pageSize, int totalCount)
	{
		Items = items;
		PageNumber = pageNumber;
		PageSize = pageSize;
		TotalCount = totalCount;
	}

	/// <summary>
	/// Boş sonuç döner.
	/// </summary>
	public static QueryResult<T> Empty(int pageNumber = 1, int pageSize = 10)
	{
		return new QueryResult<T>(new List<T>(), pageNumber, pageSize, 0);
	}
}