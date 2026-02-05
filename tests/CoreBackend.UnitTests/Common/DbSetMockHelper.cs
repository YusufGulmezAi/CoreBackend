using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace CoreBackend.UnitTests.Common;

/// <summary>
/// DbSet mock'lama yardımcısı.
/// EF Core DbSet'i mock'lamak için gerekli setup'ları yapar.
/// </summary>
public static class DbSetMockHelper
{
	/// <summary>
	/// IQueryable verisinden mock DbSet oluşturur.
	/// </summary>
	public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
	{
		var mockSet = new Mock<DbSet<T>>();

		// IQueryable setup
		mockSet.As<IQueryable<T>>()
			.Setup(m => m.Provider)
			.Returns(new TestAsyncQueryProvider<T>(data.Provider));

		mockSet.As<IQueryable<T>>()
			.Setup(m => m.Expression)
			.Returns(data.Expression);

		mockSet.As<IQueryable<T>>()
			.Setup(m => m.ElementType)
			.Returns(data.ElementType);

		mockSet.As<IQueryable<T>>()
			.Setup(m => m.GetEnumerator())
			.Returns(data.GetEnumerator());

		// IAsyncEnumerable setup (EF Core async metodları için)
		mockSet.As<IAsyncEnumerable<T>>()
			.Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
			.Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

		// NOT: AsNoTracking() extension method olduğu için mock'lanamaz
		// DbSet kendisi IQueryable olduğundan LINQ sorguları direkt çalışır

		return mockSet;
	}

	/// <summary>
	/// Boş mock DbSet oluşturur.
	/// </summary>
	public static Mock<DbSet<T>> CreateEmptyMockDbSet<T>() where T : class
	{
		return CreateMockDbSet(new List<T>().AsQueryable());
	}
}

#region Async Test Helpers

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider, IQueryProvider
{
	private readonly IQueryProvider _inner;

	internal TestAsyncQueryProvider(IQueryProvider inner)
	{
		_inner = inner;
	}

	public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
	{
		return new TestAsyncEnumerable<T>(expression);
	}

	public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
	{
		return new TestAsyncEnumerable<TElement>(expression);
	}

	public object? Execute(System.Linq.Expressions.Expression expression)
	{
		return _inner.Execute(expression);
	}

	public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
	{
		return _inner.Execute<TResult>(expression);
	}

	public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
	{
		var resultType = typeof(TResult).GetGenericArguments()[0];
		var executeMethod = typeof(IQueryProvider)
			.GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) });
		var result = executeMethod!.MakeGenericMethod(resultType).Invoke(_inner, new object[] { expression });

		return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
			.MakeGenericMethod(resultType)
			.Invoke(null, new[] { result })!;
	}
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
	public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }
	public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

	public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
	}

	IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
	private readonly IEnumerator<T> _inner;

	public TestAsyncEnumerator(IEnumerator<T> inner)
	{
		_inner = inner;
	}

	public T Current => _inner.Current;

	public ValueTask DisposeAsync()
	{
		_inner.Dispose();
		return ValueTask.CompletedTask;
	}

	public ValueTask<bool> MoveNextAsync()
	{
		return ValueTask.FromResult(_inner.MoveNext());
	}
}

#endregion