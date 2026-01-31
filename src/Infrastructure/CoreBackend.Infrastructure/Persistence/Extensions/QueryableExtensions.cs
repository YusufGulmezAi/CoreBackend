using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Models;

namespace CoreBackend.Infrastructure.Persistence.Extensions;

/// <summary>
/// IQueryable extension metodları.
/// Dynamic query, filtering, sorting, paging için.
/// </summary>
public static class QueryableExtensions
{
	/// <summary>
	/// Dinamik sorgu uygular.
	/// </summary>
	public static IQueryable<T> ApplyDynamicQuery<T>(
		this IQueryable<T> query,
		DynamicQuery? dynamicQuery) where T : class
	{
		if (dynamicQuery == null)
			return query;

		// AsNoTracking
		if (dynamicQuery.AsNoTracking)
			query = query.AsNoTracking();

		// Includes
		if (dynamicQuery.Includes.Any())
			query = query.ApplyIncludes(dynamicQuery.Includes);

		// Filters
		if (dynamicQuery.Filter != null)
			query = query.ApplyFilterGroup(dynamicQuery.Filter);

		// Sorting
		if (dynamicQuery.Sort.Any())
			query = query.ApplySorting(dynamicQuery.Sort);

		return query;
	}

	/// <summary>
	/// Include'ları uygular.
	/// </summary>
	public static IQueryable<T> ApplyIncludes<T>(
		this IQueryable<T> query,
		List<string> includes) where T : class
	{
		foreach (var include in includes)
		{
			query = query.Include(include);
		}
		return query;
	}

	/// <summary>
	/// Filtre grubunu uygular.
	/// </summary>
	public static IQueryable<T> ApplyFilterGroup<T>(
		this IQueryable<T> query,
		FilterGroup filterGroup) where T : class
	{
		var predicate = BuildFilterGroupExpression<T>(filterGroup);
		if (predicate != null)
			query = query.Where(predicate);

		return query;
	}

	/// <summary>
	/// Sıralamayı uygular.
	/// </summary>
	public static IQueryable<T> ApplySorting<T>(
		this IQueryable<T> query,
		List<SortDescriptor> sorts) where T : class
	{
		if (!sorts.Any())
			return query;

		IOrderedQueryable<T>? orderedQuery = null;

		for (int i = 0; i < sorts.Count; i++)
		{
			var sort = sorts[i];
			var parameter = Expression.Parameter(typeof(T), "x");
			var property = GetNestedProperty(parameter, sort.Field);
			var lambda = Expression.Lambda<Func<T, object>>(
				Expression.Convert(property, typeof(object)), parameter);

			if (i == 0)
			{
				orderedQuery = sort.Direction == SortDirection.Ascending
					? query.OrderBy(lambda)
					: query.OrderByDescending(lambda);
			}
			else
			{
				orderedQuery = sort.Direction == SortDirection.Ascending
					? orderedQuery!.ThenBy(lambda)
					: orderedQuery!.ThenByDescending(lambda);
			}
		}

		return orderedQuery ?? query;
	}

	/// <summary>
	/// Sayfalamayı uygular.
	/// </summary>
	public static IQueryable<T> ApplyPaging<T>(
		this IQueryable<T> query,
		int pageNumber,
		int pageSize) where T : class
	{
		return query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize);
	}

	/// <summary>
	/// Hızlı arama uygular (birden fazla alanda).
	/// </summary>
	public static IQueryable<T> ApplySearch<T>(
		this IQueryable<T> query,
		string? searchText,
		List<string> searchFields) where T : class
	{
		if (string.IsNullOrWhiteSpace(searchText) || !searchFields.Any())
			return query;

		var parameter = Expression.Parameter(typeof(T), "x");
		Expression? combinedExpression = null;

		foreach (var field in searchFields)
		{
			try
			{
				var property = GetNestedProperty(parameter, field);

				// Sadece string alanlar için Contains uygula
				if (property.Type == typeof(string))
				{
					var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
					var searchValue = Expression.Constant(searchText, typeof(string));

					// Null check ekle
					var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
					var containsCall = Expression.Call(property, containsMethod, searchValue);
					var safeContains = Expression.AndAlso(nullCheck, containsCall);

					combinedExpression = combinedExpression == null
						? safeContains
						: Expression.OrElse(combinedExpression, safeContains);
				}
			}
			catch
			{
				// Geçersiz alan adını atla
				continue;
			}
		}

		if (combinedExpression != null)
		{
			var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
			query = query.Where(lambda);
		}

		return query;
	}

	/// <summary>
	/// Filtre grubu için expression oluşturur.
	/// </summary>
	private static Expression<Func<T, bool>>? BuildFilterGroupExpression<T>(FilterGroup filterGroup)
	{
		var parameter = Expression.Parameter(typeof(T), "x");
		var expression = BuildFilterGroupExpressionRecursive<T>(filterGroup, parameter);

		if (expression == null)
			return null;

		return Expression.Lambda<Func<T, bool>>(expression, parameter);
	}

	/// <summary>
	/// Filtre grubu için recursive expression oluşturur.
	/// </summary>
	private static Expression? BuildFilterGroupExpressionRecursive<T>(
		FilterGroup filterGroup,
		ParameterExpression parameter)
	{
		Expression? result = null;

		// Filtreleri işle
		foreach (var filter in filterGroup.Filters)
		{
			var filterExpression = BuildFilterExpression<T>(filter, parameter);
			if (filterExpression != null)
			{
				result = result == null
					? filterExpression
					: CombineExpressions(result, filterExpression, filterGroup.Logic);
			}
		}

		// Alt grupları işle
		foreach (var subGroup in filterGroup.Groups)
		{
			var subExpression = BuildFilterGroupExpressionRecursive<T>(subGroup, parameter);
			if (subExpression != null)
			{
				result = result == null
					? subExpression
					: CombineExpressions(result, subExpression, filterGroup.Logic);
			}
		}

		return result;
	}

	/// <summary>
	/// Tekil filtre için expression oluşturur.
	/// </summary>
	private static Expression? BuildFilterExpression<T>(
		FilterDescriptor filter,
		ParameterExpression parameter)
	{
		try
		{
			var property = GetNestedProperty(parameter, filter.Field);
			var propertyType = property.Type;
			var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

			// IsNull ve IsNotNull için özel işlem
			if (filter.Operator == FilterOperator.IsNull)
			{
				return Expression.Equal(property, Expression.Constant(null, propertyType));
			}

			if (filter.Operator == FilterOperator.IsNotNull)
			{
				return Expression.NotEqual(property, Expression.Constant(null, propertyType));
			}

			// Değer dönüşümü
			var value = ConvertValue(filter.Value, underlyingType);
			var constant = Expression.Constant(value, propertyType);

			return filter.Operator switch
			{
				FilterOperator.Equals => Expression.Equal(property, constant),
				FilterOperator.NotEquals => Expression.NotEqual(property, constant),
				FilterOperator.GreaterThan => Expression.GreaterThan(property, constant),
				FilterOperator.GreaterThanOrEquals => Expression.GreaterThanOrEqual(property, constant),
				FilterOperator.LessThan => Expression.LessThan(property, constant),
				FilterOperator.LessThanOrEquals => Expression.LessThanOrEqual(property, constant),
				FilterOperator.Contains => BuildStringContainsExpression(property, filter),
				FilterOperator.StartsWith => BuildStringStartsWithExpression(property, filter),
				FilterOperator.EndsWith => BuildStringEndsWithExpression(property, filter),
				FilterOperator.Between => BuildBetweenExpression(property, filter, underlyingType),
				FilterOperator.In => BuildInExpression(property, filter, underlyingType),
				FilterOperator.NotIn => Expression.Not(BuildInExpression(property, filter, underlyingType)!),
				_ => null
			};
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// String Contains expression oluşturur.
	/// </summary>
	private static Expression? BuildStringContainsExpression(
		Expression property,
		FilterDescriptor filter)
	{
		if (property.Type != typeof(string) || filter.Value == null)
			return null;

		var value = filter.Value.ToString()!;

		if (!filter.IsCaseSensitive)
		{
			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
			property = Expression.Call(property, toLowerMethod);
			value = value.ToLower();
		}

		var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
		var constant = Expression.Constant(value, typeof(string));

		var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
		var containsCall = Expression.Call(property, containsMethod, constant);

		return Expression.AndAlso(nullCheck, containsCall);
	}

	/// <summary>
	/// String StartsWith expression oluşturur.
	/// </summary>
	private static Expression? BuildStringStartsWithExpression(
		Expression property,
		FilterDescriptor filter)
	{
		if (property.Type != typeof(string) || filter.Value == null)
			return null;

		var value = filter.Value.ToString()!;

		if (!filter.IsCaseSensitive)
		{
			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
			property = Expression.Call(property, toLowerMethod);
			value = value.ToLower();
		}

		var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
		var constant = Expression.Constant(value, typeof(string));

		var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
		var startsWithCall = Expression.Call(property, startsWithMethod, constant);

		return Expression.AndAlso(nullCheck, startsWithCall);
	}

	/// <summary>
	/// String EndsWith expression oluşturur.
	/// </summary>
	private static Expression? BuildStringEndsWithExpression(
		Expression property,
		FilterDescriptor filter)
	{
		if (property.Type != typeof(string) || filter.Value == null)
			return null;

		var value = filter.Value.ToString()!;

		if (!filter.IsCaseSensitive)
		{
			var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
			property = Expression.Call(property, toLowerMethod);
			value = value.ToLower();
		}

		var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;
		var constant = Expression.Constant(value, typeof(string));

		var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
		var endsWithCall = Expression.Call(property, endsWithMethod, constant);

		return Expression.AndAlso(nullCheck, endsWithCall);
	}

	/// <summary>
	/// Between expression oluşturur.
	/// </summary>
	private static Expression? BuildBetweenExpression(
		Expression property,
		FilterDescriptor filter,
		Type underlyingType)
	{
		if (filter.Value == null || filter.ValueTo == null)
			return null;

		var fromValue = ConvertValue(filter.Value, underlyingType);
		var toValue = ConvertValue(filter.ValueTo, underlyingType);

		var fromConstant = Expression.Constant(fromValue, property.Type);
		var toConstant = Expression.Constant(toValue, property.Type);

		var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, fromConstant);
		var lessThanOrEqual = Expression.LessThanOrEqual(property, toConstant);

		return Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
	}

	/// <summary>
	/// In expression oluşturur.
	/// </summary>
	private static Expression? BuildInExpression(
		Expression property,
		FilterDescriptor filter,
		Type underlyingType)
	{
		if (filter.Value == null)
			return null;

		var values = filter.Value as IEnumerable<object>;
		if (values == null)
			return null;

		var convertedValues = values.Select(v => ConvertValue(v, underlyingType)).ToList();
		var listType = typeof(List<>).MakeGenericType(underlyingType);
		var list = Activator.CreateInstance(listType);
		var addMethod = listType.GetMethod("Add")!;

		foreach (var value in convertedValues)
		{
			addMethod.Invoke(list, new[] { value });
		}

		var containsMethod = listType.GetMethod("Contains")!;
		var listConstant = Expression.Constant(list, listType);

		return Expression.Call(listConstant, containsMethod, property);
	}

	/// <summary>
	/// İki expression'ı birleştirir.
	/// </summary>
	private static Expression CombineExpressions(
		Expression left,
		Expression right,
		FilterLogic logic)
	{
		return logic == FilterLogic.And
			? Expression.AndAlso(left, right)
			: Expression.OrElse(left, right);
	}

	/// <summary>
	/// Nested property için expression oluşturur.
	/// </summary>
	private static Expression GetNestedProperty(Expression parameter, string propertyPath)
	{
		Expression property = parameter;
		foreach (var member in propertyPath.Split('.'))
		{
			property = Expression.PropertyOrField(property, member);
		}
		return property;
	}

	/// <summary>
	/// Değeri hedef tipe dönüştürür.
	/// </summary>
	private static object? ConvertValue(object? value, Type targetType)
	{
		if (value == null)
			return null;

		if (targetType == typeof(Guid) && value is string stringValue)
			return Guid.Parse(stringValue);

		if (targetType == typeof(DateTime) && value is string dateString)
			return DateTime.Parse(dateString);

		if (targetType.IsEnum && value is string enumString)
			return Enum.Parse(targetType, enumString);

		return Convert.ChangeType(value, targetType);
	}
}