namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Tek bir filtre tanımlayıcı.
/// </summary>
public class FilterDescriptor
{
	/// <summary>
	/// Filtrelenecek alan adı.
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
	/// Dize karşılaştırmalarının büyük/küçük harf duyarlı olup olmadığını belirten bir değer alır veya ayarlar.
	/// </summary>
	public bool IsCaseSensitive { get; set; } = false;

	/// <summary>
	/// İkinci değer (Between operatörü için).
	/// </summary>
	public object? ValueTo { get; set; }

	public FilterDescriptor() { }

	public FilterDescriptor(string field, FilterOperator @operator, object? value)
	{
		Field = field;
		Operator = @operator;
		Value = value;
	}

	/// <summary>
	/// Equals filtresi oluşturur.
	/// </summary>
	public static FilterDescriptor Equals(string field, object? value)
		=> new(field, FilterOperator.Equals, value);

	/// <summary>
	/// Contains filtresi oluşturur.
	/// </summary>
	public static FilterDescriptor Contains(string field, string value)
		=> new(field, FilterOperator.Contains, value);

	/// <summary>
	/// GreaterThan filtresi oluşturur.
	/// </summary>
	public static FilterDescriptor GreaterThan(string field, object value)
		=> new(field, FilterOperator.GreaterThan, value);

	/// <summary>
	/// LessThan filtresi oluşturur.
	/// </summary>
	public static FilterDescriptor LessThan(string field, object value)
		=> new(field, FilterOperator.LessThan, value);
}

/// <summary>
/// Filtre operatörleri.
/// </summary>
public enum FilterOperator
{
	/// <summary>Eşit (=)</summary>
	Equals,

	/// <summary>Eşit değil (!=)</summary>
	NotEquals,

	/// <summary>İçerir (LIKE %value%)</summary>
	Contains,

	/// <summary>İçermez</summary>
	NotContains,

	/// <summary>İle başlar (LIKE value%)</summary>
	StartsWith,

	/// <summary>İle biter (LIKE %value)</summary>
	EndsWith,

	/// <summary>Büyüktür (>)</summary>
	GreaterThan,

	/// <summary>Büyük eşit (>=)</summary>
	GreaterThanOrEquals,

	/// <summary>Küçüktür (<)</summary>
	LessThan,

	/// <summary>Küçük eşit (<=)</summary>
	LessThanOrEquals,

	/// <summary>Arasında (BETWEEN)</summary>
	Between,

	/// <summary>Liste içinde (IN)</summary>
	In,

	/// <summary>Liste dışında (NOT IN)</summary>
	NotIn,

	/// <summary>Null</summary>
	IsNull,

	/// <summary>Null değil</summary>
	IsNotNull
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