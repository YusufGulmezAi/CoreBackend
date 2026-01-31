namespace CoreBackend.Domain.Common.Primitives;

/// <summary>
/// Tüm entity'lerin temel sınıfı.
/// </summary>
public abstract class BaseEntity<TId> where TId : notnull
{
	public TId Id { get; protected set; } = default!;

	protected BaseEntity() { }

	protected BaseEntity(TId id)
	{
		Id = id;
	}

	public override bool Equals(object? obj)
	{
		if (obj is not BaseEntity<TId> other)
			return false;

		if (ReferenceEquals(this, other))
			return true;

		if (Id.Equals(default(TId)) || other.Id.Equals(default(TId)))
			return false;

		return Id.Equals(other.Id);
	}

	public override int GetHashCode() => Id.GetHashCode();

	public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
	{
		if (left is null && right is null) return true;
		if (left is null || right is null) return false;
		return left.Equals(right);
	}

	public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
	{
		return !(left == right);
	}
}