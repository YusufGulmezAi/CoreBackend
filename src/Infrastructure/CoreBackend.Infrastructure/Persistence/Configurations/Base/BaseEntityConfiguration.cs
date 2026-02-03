using CoreBackend.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreBackend.Infrastructure.Persistence.Configurations.Base;

/// <summary>
/// Tenant entity'leri için base configuration.
/// Sadece Soft Delete query filter ve index uygular.
/// Relationship tanımlamaz - her entity kendi relationship'ini tanımlar.
/// </summary>
public abstract class TenantEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
	where TEntity : class, ITenantEntity, ISoftDeletable
{
	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		// Soft Delete Query Filter
		builder.HasQueryFilter(e => !e.IsDeleted);

		// NOT: Relationship burada tanımlanmıyor
		// Her entity kendi configuration'ında Tenant relationship'ini tanımlar

		// NOT: TenantId index'i her entity'de ayrı tanımlanır (filtered index ile)
	}
}

/// <summary>
/// Soft delete entity'leri için base configuration (Tenant olmadan).
/// </summary>
public abstract class SoftDeleteEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
	where TEntity : class, ISoftDeletable
{
	public virtual void Configure(EntityTypeBuilder<TEntity> builder)
	{
		// Soft Delete Query Filter
		builder.HasQueryFilter(e => !e.IsDeleted);
	}
}