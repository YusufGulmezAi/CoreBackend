namespace CoreBackend.Contracts.Common;

/// <summary>
/// API sayfalama isteği DTO.
/// Frontend'den gelen istekleri karşılar.
/// </summary>
public class PaginationRequest
{
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 10;
	public string? SearchText { get; set; }
	public string? SortBy { get; set; }
	public bool SortDescending { get; set; } = false;
}