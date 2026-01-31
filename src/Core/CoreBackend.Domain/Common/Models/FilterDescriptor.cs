namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Tekil filtre tanımı.
/// </summary>
public class FilterDescriptor
{
	/// <summary>
	/// Filtrelenecek alan adı (örn: "Name", "Status", "CreatedAt").
	/// </summary>
	public string Field { get; set; } = null!;

	/// <summary>
	/// Filtre operatörü.
	/// </summary>
	public FilterOperator Operator { get; set; } = FilterOperator.Equals;

	/// <summary>
	/// Filtre değeri.
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// İkinci değer (Between operatörü için).
	/// </summary>
	public object? ValueTo { get; set; }

	/// <summary>
	/// Büyük/küçük harf duyarlılığı (string için).
	/// </summary>
	public bool IsCaseSensitive { get; set; } = false;
}

/// <summary>
/// Filtre grubu (AND/OR kombinasyonları için).
/// </summary>
public class FilterGroup
{
	/// <summary>
	/// Grup içindeki filtreler.
	/// </summary>
	public List<FilterDescriptor> Filters { get; set; } = new();

	/// <summary>
	/// Alt gruplar (nested filtering için).
	/// </summary>
	public List<FilterGroup> Groups { get; set; } = new();

	/// <summary>
	/// Grup içi mantıksal operatör (AND/OR).
	/// </summary>
	public FilterLogic Logic { get; set; } = FilterLogic.And;
}

/// <summary>
/// Mantıksal operatör.
/// </summary>
public enum FilterLogic
{
	And,
	Or
}