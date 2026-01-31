namespace CoreBackend.Domain.Errors;

/// <summary>
/// Hata bilgisini temsil eder.
/// Kod, mesaj ve parametreler içerir.
/// </summary>
public sealed class Error : IEquatable<Error>
{
	public string Code { get; }
	public string Message { get; }
	public Dictionary<string, object>? Parameters { get; }

	private Error(string code, string message, Dictionary<string, object>? parameters = null)
	{
		Code = code;
		Message = message;
		Parameters = parameters;
	}

	public static Error Create(string code, string message, Dictionary<string, object>? parameters = null)
		=> new(code, message, parameters);

	public static Error Create(string code, string message, object parameters)
		=> new(code, message, ConvertToDictionary(parameters));

	public static Error None => new(string.Empty, string.Empty);

	public static Error NullValue => new("NULL_VALUE", "Null değer sağlandı.");

	private static Dictionary<string, object>? ConvertToDictionary(object? obj)
	{
		if (obj is null) return null;

		return obj.GetType()
			.GetProperties()
			.ToDictionary(p => p.Name, p => p.GetValue(obj) ?? "null");
	}

	public bool Equals(Error? other)
	{
		if (other is null) return false;
		return Code == other.Code;
	}

	public override bool Equals(object? obj) => obj is Error error && Equals(error);

	public override int GetHashCode() => Code.GetHashCode();

	public override string ToString() => Code;

	public static bool operator ==(Error? left, Error? right)
	{
		if (left is null && right is null) return true;
		if (left is null || right is null) return false;
		return left.Equals(right);
	}

	public static bool operator !=(Error? left, Error? right) => !(left == right);
}