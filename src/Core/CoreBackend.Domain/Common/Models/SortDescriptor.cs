namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Sıralama tanımı.
/// </summary>
public class SortDescriptor
{
	/// <summary>
	/// Sıralanacak alan adı.
	/// </summary>
	public string Field { get; set; } = null!;

	/// <summary>
	/// Sıralama yönü.
	/// </summary>
	public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

/// <summary>
/// Sıralama yönü.
/// </summary>
public enum SortDirection
{
	Ascending,
	Descending
}