namespace CoreBackend.Application.Common.Models;

/// <summary>
/// Filtre grubu.
/// Karmaşık AND/OR sorguları için kullanılır.
/// </summary>
public class FilterGroup
{
	/// <summary>
	/// Gruptaki filtreler.
	/// </summary>
	public List<FilterDescriptor> Filters { get; set; } = new();

	/// <summary>
	/// Alt gruplar (iç içe gruplar için).
	/// </summary>
	public List<FilterGroup>? SubGroups { get; set; }

	/// <summary>
	/// Grup içi mantıksal operatör.
	/// Filtreler kendi aralarında bu operatörle birleştirilir.
	/// </summary>
	public LogicalOperator Logic { get; set; } = LogicalOperator.And;

	public FilterGroup() { }

	public FilterGroup(LogicalOperator logic, params FilterDescriptor[] filters)
	{
		Logic = logic;
		Filters = filters.ToList();
	}

	/// <summary>
	/// AND grubu oluşturur.
	/// </summary>
	public static FilterGroup And(params FilterDescriptor[] filters)
		=> new(LogicalOperator.And, filters);

	/// <summary>
	/// OR grubu oluşturur.
	/// </summary>
	public static FilterGroup Or(params FilterDescriptor[] filters)
		=> new(LogicalOperator.Or, filters);
}

/// <summary>
/// Mantıksal operatörler.
/// </summary>
public enum LogicalOperator
{
	/// <summary>VE (AND)</summary>
	And,

	/// <summary>VEYA (OR)</summary>
	Or
}