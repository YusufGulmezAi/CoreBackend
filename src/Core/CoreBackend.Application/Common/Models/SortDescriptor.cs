namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Sıralama tanımlayıcı.
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

	public SortDescriptor() { }

	public SortDescriptor(string field, SortDirection direction = SortDirection.Ascending)
	{
		Field = field;
		Direction = direction;
	}

	/// <summary>
	/// Artan sıralama oluşturur.
	/// </summary>
	public static SortDescriptor Asc(string field) => new(field, SortDirection.Ascending);

	/// <summary>
	/// Azalan sıralama oluşturur.
	/// </summary>
	public static SortDescriptor Desc(string field) => new(field, SortDirection.Descending);
}

/// <summary>
/// Sıralama yönü.
/// </summary>
public enum SortDirection
{
	/// <summary>Artan (A-Z, 0-9)</summary>
	Ascending,

	/// <summary>Azalan (Z-A, 9-0)</summary>
	Descending
}