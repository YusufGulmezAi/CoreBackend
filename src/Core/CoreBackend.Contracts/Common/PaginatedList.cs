namespace CoreBackend.Contracts.Common;

/// <summary>
/// Sayfalanmış liste sonucu.
/// API response olarak kullanılır.
/// </summary>
public class PaginatedList<T>
{
	public List<T> Items { get; set; } = new();
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalCount { get; set; }
	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
	public bool HasPreviousPage => PageNumber > 1;
	public bool HasNextPage => PageNumber < TotalPages;
}