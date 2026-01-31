namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Filtreleme operatörleri.
/// Frontend'den gelen filter işlemleri için.
/// </summary>
public enum FilterOperator
{
	/// <summary>
	/// Eşittir (==)
	/// </summary>
	Equals,

	/// <summary>
	/// Eşit değildir (!=)
	/// </summary>
	NotEquals,

	/// <summary>
	/// İçerir (LIKE %value%)
	/// </summary>
	Contains,

	/// <summary>
	/// İle başlar (LIKE value%)
	/// </summary>
	StartsWith,

	/// <summary>
	/// İle biter (LIKE %value)
	/// </summary>
	EndsWith,

	/// <summary>
	/// Büyüktür (>)
	/// </summary>
	GreaterThan,

	/// <summary>
	/// Büyük veya eşittir (>=)
	/// </summary>
	GreaterThanOrEquals,

	/// <summary>
	/// Küçüktür (<)
	/// </summary>
	LessThan,

	/// <summary>
	/// Küçük veya eşittir (<=)
	/// </summary>
	LessThanOrEquals,

	/// <summary>
	/// Arasında (BETWEEN)
	/// </summary>
	Between,

	/// <summary>
	/// Liste içinde (IN)
	/// </summary>
	In,

	/// <summary>
	/// Liste içinde değil (NOT IN)
	/// </summary>
	NotIn,

	/// <summary>
	/// Null mu?
	/// </summary>
	IsNull,

	/// <summary>
	/// Null değil mi?
	/// </summary>
	IsNotNull
}