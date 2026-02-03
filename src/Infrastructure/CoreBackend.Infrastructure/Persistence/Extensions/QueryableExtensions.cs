using System.Linq.Expressions;
using System.Reflection;
using CoreBackend.Application.Common.Models;

namespace CoreBackend.Infrastructure.Persistence.Extensions;

/// <summary>
/// IQueryable extension metodları.
/// Dinamik filtreleme, sıralama ve arama için.
/// </summary>
public static class QueryableExtensions
{
	#region Search

	/// <summary>
	/// Metin araması uygular.
	/// </summary>
	public static IQueryable<T> ApplySearch<T>(
		this IQueryable<T> query,
		string? searchText,
		List<string>? searchFields)
	{
		if (string.IsNullOrWhiteSpace(searchText) || searchFields == null || !searchFields.Any())
			return query;

		var parameter = Expression.Parameter(typeof(T), "x");
		Expression? combinedExpression = null;

		foreach (var field in searchFields)
		{
			var property = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (property == null || property.PropertyType != typeof(string))
				continue;

			var propertyAccess = Expression.Property(parameter, property);
			var searchValue = Expression.Constant(searchText.ToLower());

			// property != null && property.ToLower().Contains(searchText.ToLower())
			var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));

			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
			var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

			var toLowerCall = Expression.Call(propertyAccess, toLowerMethod);
			var containsCall = Expression.Call(toLowerCall, containsMethod, searchValue);

			var safeContains = Expression.AndAlso(nullCheck, containsCall);

			combinedExpression = combinedExpression == null
				? safeContains
				: Expression.OrElse(combinedExpression, safeContains);
		}

		if (combinedExpression == null)
			return query;

		var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
		return query.Where(lambda);
	}

	#endregion

	#region Filter

	/// <summary>
	/// Filtreleri uygular.
	/// </summary>
	public static IQueryable<T> ApplyFilters<T>(
		this IQueryable<T> query,
		List<FilterDescriptor>? filters)
	{
		if (filters == null || !filters.Any())
			return query;

		foreach (var filter in filters)
		{
			var predicate = BuildFilterExpression<T>(filter);
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
		}

		return query;
	}

	/// <summary>
	/// Filtre gruplarını uygular.
	/// </summary>
	public static IQueryable<T> ApplyFilterGroups<T>(
		this IQueryable<T> query,
		List<FilterGroup>? filterGroups)
	{
		if (filterGroups == null || !filterGroups.Any())
			return query;

		foreach (var group in filterGroups)
		{
			var predicate = BuildFilterGroupExpression<T>(group);
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
		}

		return query;
	}

	/// <summary>
	/// Tek filtre için expression oluşturur.
	/// </summary>
	private static Expression<Func<T, bool>>? BuildFilterExpression<T>(FilterDescriptor filter)
	{
		var parameter = Expression.Parameter(typeof(T), "x");
		var property = typeof(T).GetProperty(filter.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

		if (property == null)
			return null;

		var propertyAccess = Expression.Property(parameter, property);

		Expression? comparison = filter.Operator switch
		{
			FilterOperator.Equals => BuildEqualsExpression(propertyAccess, filter.Value, property.PropertyType),
			FilterOperator.NotEquals => BuildNotEqualsExpression(propertyAccess, filter.Value, property.PropertyType),
			FilterOperator.Contains => BuildContainsExpression(propertyAccess, filter.Value?.ToString()),
			FilterOperator.NotContains => BuildNotContainsExpression(propertyAccess, filter.Value?.ToString()),
			FilterOperator.StartsWith => BuildStartsWithExpression(propertyAccess, filter.Value?.ToString()),
			FilterOperator.EndsWith => BuildEndsWithExpression(propertyAccess, filter.Value?.ToString()),
			FilterOperator.GreaterThan => BuildComparisonExpression(propertyAccess, filter.Value, property.PropertyType, ExpressionType.GreaterThan),
			FilterOperator.GreaterThanOrEquals => BuildComparisonExpression(propertyAccess, filter.Value, property.PropertyType, ExpressionType.GreaterThanOrEqual),
			FilterOperator.LessThan => BuildComparisonExpression(propertyAccess, filter.Value, property.PropertyType, ExpressionType.LessThan),
			FilterOperator.LessThanOrEquals => BuildComparisonExpression(propertyAccess, filter.Value, property.PropertyType, ExpressionType.LessThanOrEqual),
			FilterOperator.Between => BuildBetweenExpression(propertyAccess, filter.Value, filter.ValueTo, property.PropertyType),
			FilterOperator.In => BuildInExpression(propertyAccess, filter.Value, property.PropertyType),
			FilterOperator.NotIn => BuildNotInExpression(propertyAccess, filter.Value, property.PropertyType),
			FilterOperator.IsNull => Expression.Equal(propertyAccess, Expression.Constant(null, property.PropertyType)),
			FilterOperator.IsNotNull => Expression.NotEqual(propertyAccess, Expression.Constant(null, property.PropertyType)),
			_ => null
		};

		if (comparison == null)
			return null;

		return Expression.Lambda<Func<T, bool>>(comparison, parameter);
	}

	/// <summary>
	/// Filtre grubu için expression oluşturur.
	/// </summary>
	private static Expression<Func<T, bool>>? BuildFilterGroupExpression<T>(FilterGroup group)
	{
		var parameter = Expression.Parameter(typeof(T), "x");
		Expression? combinedExpression = null;

		// Gruptaki filtreleri işle
		foreach (var filter in group.Filters)
		{
			var filterExpression = BuildFilterExpression<T>(filter);
			if (filterExpression == null)
				continue;

			var invokedExpr = Expression.Invoke(filterExpression, parameter);

			combinedExpression = combinedExpression == null
				? invokedExpr
				: group.Logic == LogicalOperator.And
					? Expression.AndAlso(combinedExpression, invokedExpr)
					: Expression.OrElse(combinedExpression, invokedExpr);
		}

		// Alt grupları işle
		if (group.SubGroups != null)
		{
			foreach (var subGroup in group.SubGroups)
			{
				var subGroupExpression = BuildFilterGroupExpression<T>(subGroup);
				if (subGroupExpression == null)
					continue;

				var invokedExpr = Expression.Invoke(subGroupExpression, parameter);

				combinedExpression = combinedExpression == null
					? invokedExpr
					: group.Logic == LogicalOperator.And
						? Expression.AndAlso(combinedExpression, invokedExpr)
						: Expression.OrElse(combinedExpression, invokedExpr);
			}
		}

		if (combinedExpression == null)
			return null;

		return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
	}

	#region Filter Expression Builders

	private static Expression? BuildEqualsExpression(MemberExpression property, object? value, Type propertyType)
	{
		if (value == null)
			return Expression.Equal(property, Expression.Constant(null, propertyType));

		var convertedValue = ConvertValue(value, propertyType);
		if (convertedValue == null)
			return null;

		return Expression.Equal(property, Expression.Constant(convertedValue, propertyType));
	}

	private static Expression? BuildNotEqualsExpression(MemberExpression property, object? value, Type propertyType)
	{
		if (value == null)
			return Expression.NotEqual(property, Expression.Constant(null, propertyType));

		var convertedValue = ConvertValue(value, propertyType);
		if (convertedValue == null)
			return null;

		return Expression.NotEqual(property, Expression.Constant(convertedValue, propertyType));
	}

	private static Expression? BuildContainsExpression(MemberExpression property, string? value)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
		var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
		var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

		var toLowerCall = Expression.Call(property, toLowerMethod);
		var containsCall = Expression.Call(toLowerCall, containsMethod, Expression.Constant(value.ToLower()));

		return Expression.AndAlso(nullCheck, containsCall);
	}

	private static Expression? BuildNotContainsExpression(MemberExpression property, string? value)
	{
		var containsExpr = BuildContainsExpression(property, value);
		return containsExpr != null ? Expression.Not(containsExpr) : null;
	}

	private static Expression? BuildStartsWithExpression(MemberExpression property, string? value)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
		var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) })!;
		var startsWithCall = Expression.Call(property, startsWithMethod,
			Expression.Constant(value),
			Expression.Constant(StringComparison.OrdinalIgnoreCase));

		return Expression.AndAlso(nullCheck, startsWithCall);
	}

	private static Expression? BuildEndsWithExpression(MemberExpression property, string? value)
	{
		if (string.IsNullOrEmpty(value))
			return null;

		var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
		var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) })!;
		var endsWithCall = Expression.Call(property, endsWithMethod,
			Expression.Constant(value),
			Expression.Constant(StringComparison.OrdinalIgnoreCase));

		return Expression.AndAlso(nullCheck, endsWithCall);
	}

	private static Expression? BuildComparisonExpression(MemberExpression property, object? value, Type propertyType, ExpressionType comparisonType)
	{
		if (value == null)
			return null;

		var convertedValue = ConvertValue(value, propertyType);
		if (convertedValue == null)
			return null;

		var constant = Expression.Constant(convertedValue, propertyType);

		return comparisonType switch
		{
			ExpressionType.GreaterThan => Expression.GreaterThan(property, constant),
			ExpressionType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
			ExpressionType.LessThan => Expression.LessThan(property, constant),
			ExpressionType.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
			_ => null
		};
	}

	private static Expression? BuildBetweenExpression(MemberExpression property, object? valueFrom, object? valueTo, Type propertyType)
	{
		if (valueFrom == null || valueTo == null)
			return null;

		var convertedFrom = ConvertValue(valueFrom, propertyType);
		var convertedTo = ConvertValue(valueTo, propertyType);

		if (convertedFrom == null || convertedTo == null)
			return null;

		var fromConstant = Expression.Constant(convertedFrom, propertyType);
		var toConstant = Expression.Constant(convertedTo, propertyType);

		var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, fromConstant);
		var lessThanOrEqual = Expression.LessThanOrEqual(property, toConstant);

		return Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
	}

	private static Expression? BuildInExpression(MemberExpression property, object? value, Type propertyType)
	{
		if (value == null)
			return null;

		// Value should be IEnumerable
		var enumerable = value as System.Collections.IEnumerable;
		if (enumerable == null)
			return null;

		var values = enumerable.Cast<object>().ToList();
		if (!values.Any())
			return null;

		Expression? combinedExpression = null;
		var parameter = property.Expression as ParameterExpression;

		foreach (var val in values)
		{
			var convertedValue = ConvertValue(val, propertyType);
			if (convertedValue == null)
				continue;

			var constant = Expression.Constant(convertedValue, propertyType);
			var equality = Expression.Equal(property, constant);

			combinedExpression = combinedExpression == null
				? equality
				: Expression.OrElse(combinedExpression, equality);
		}

		return combinedExpression;
	}

	private static Expression? BuildNotInExpression(MemberExpression property, object? value, Type propertyType)
	{
		var inExpr = BuildInExpression(property, value, propertyType);
		return inExpr != null ? Expression.Not(inExpr) : null;
	}

	#endregion

	#endregion

	#region Sort

	/// <summary>
	/// Sıralama uygular.
	/// </summary>
	public static IQueryable<T> ApplySort<T>(
		this IQueryable<T> query,
		List<SortDescriptor>? sorts)
	{
		if (sorts == null || !sorts.Any())
			return query;

		IOrderedQueryable<T>? orderedQuery = null;

		foreach (var sort in sorts)
		{
			var parameter = Expression.Parameter(typeof(T), "x");
			var property = typeof(T).GetProperty(sort.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
				continue;

			var propertyAccess = Expression.Property(parameter, property);
			var lambda = Expression.Lambda<Func<T, object>>(
				Expression.Convert(propertyAccess, typeof(object)),
				parameter);

			if (orderedQuery == null)
			{
				orderedQuery = sort.Direction == SortDirection.Ascending
					? query.OrderBy(lambda)
					: query.OrderByDescending(lambda);
			}
			else
			{
				orderedQuery = sort.Direction == SortDirection.Ascending
					? orderedQuery.ThenBy(lambda)
					: orderedQuery.ThenByDescending(lambda);
			}
		}

		return orderedQuery ?? query;
	}

	/// <summary>
	/// Varsayılan sıralama uygular (CreatedAt DESC veya Id DESC).
	/// </summary>
	public static IQueryable<T> ApplyDefaultSort<T>(this IQueryable<T> query)
	{
		var parameter = Expression.Parameter(typeof(T), "x");

		// Önce CreatedAt dene
		var property = typeof(T).GetProperty("CreatedAt", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

		// CreatedAt yoksa Id dene
		if (property == null)
		{
			property = typeof(T).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
		}

		if (property == null)
			return query;

		var propertyAccess = Expression.Property(parameter, property);
		var lambda = Expression.Lambda<Func<T, object>>(
			Expression.Convert(propertyAccess, typeof(object)),
			parameter);

		return query.OrderByDescending(lambda);
	}

	#endregion

	#region Pagination

	/// <summary>
	/// Sayfalama uygular.
	/// </summary>
	public static IQueryable<T> ApplyPaging<T>(
		this IQueryable<T> query,
		int pageNumber,
		int pageSize)
	{
		if (pageNumber < 1) pageNumber = 1;
		if (pageSize < 1) pageSize = 10;
		if (pageSize > 100) pageSize = 100; // Max limit

		return query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize);
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Değeri belirtilen tipe dönüştürür.
	/// </summary>
	private static object? ConvertValue(object? value, Type targetType)
	{
		if (value == null)
			return null;

		try
		{
			// Nullable tip kontrolü
			var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

			// Guid dönüşümü
			if (underlyingType == typeof(Guid))
			{
				return value is Guid guid ? guid : Guid.Parse(value.ToString()!);
			}

			// Enum dönüşümü
			if (underlyingType.IsEnum)
			{
				return Enum.Parse(underlyingType, value.ToString()!, ignoreCase: true);
			}

			// DateTime dönüşümü
			if (underlyingType == typeof(DateTime))
			{
				return value is DateTime dt ? dt : DateTime.Parse(value.ToString()!);
			}

			// DateOnly dönüşümü
			if (underlyingType == typeof(DateOnly))
			{
				return value is DateOnly d ? d : DateOnly.Parse(value.ToString()!);
			}

			// Genel dönüşüm
			return Convert.ChangeType(value, underlyingType);
		}
		catch
		{
			return null;
		}
	}

	#endregion
}