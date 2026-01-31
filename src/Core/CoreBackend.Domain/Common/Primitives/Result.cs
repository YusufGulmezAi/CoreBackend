using CoreBackend.Domain.Errors;

namespace CoreBackend.Domain.Common.Primitives;

/// <summary>
/// İşlem sonucunu temsil eder. Başarılı veya başarısız.
/// Exception yerine Result döndürerek daha temiz hata yönetimi sağlar.
/// </summary>
public class Result
{
	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	public Error Error { get; }

	protected Result(bool isSuccess, Error error)
	{
		IsSuccess = isSuccess;
		Error = error;
	}

	public static Result Success() => new(true, Error.None);

	public static Result Failure(Error error) => new(false, error);

	public static Result Failure(string code, string message)
		=> new(false, Error.Create(code, message));

	public static Result Failure(string code, string message, object parameters)
		=> new(false, Error.Create(code, message, parameters));

	public static Result<T> Success<T>(T value) => Result<T>.Success(value);

	public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

	public static Result<T> Failure<T>(string code, string message)
		=> Result<T>.Failure(Error.Create(code, message));

	public static Result<T> Failure<T>(string code, string message, object parameters)
		=> Result<T>.Failure(Error.Create(code, message, parameters));
}

/// <summary>
/// Generic Result. Başarılı durumda değer de taşır.
/// </summary>
public class Result<T> : Result
{
	private readonly T? _value;

	public T Value => IsSuccess
		? _value!
		: throw new InvalidOperationException("Başarısız sonuçtan değer alınamaz.");

	private Result(bool isSuccess, T? value, Error error)
		: base(isSuccess, error)
	{
		_value = value;
	}

	public static Result<T> Success(T value) => new(true, value, Error.None);

	public new static Result<T> Failure(Error error) => new(false, default, error);

	public static new Result<T> Failure(string code, string message)
		=> new(false, default, Error.Create(code, message));

	public static new Result<T> Failure(string code, string message, object parameters)
		=> new(false, default, Error.Create(code, message, parameters));
}