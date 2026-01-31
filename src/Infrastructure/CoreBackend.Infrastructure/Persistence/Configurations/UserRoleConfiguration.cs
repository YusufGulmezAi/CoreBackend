using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// UserRole entity konfigürasyonu.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
	public void Configure(EntityTypeBuilder<UserRole> builder)
	{
		// Tablo adı
		builder.ToTable("UserRoles");

		// Primary Key
		builder.HasKey(ur => ur.Id);

		// Properties
		builder.Property(ur => ur.UserId)
			.IsRequired();

		builder.Property(ur => ur.RoleId)
			.IsRequired();

		builder.Property(ur => ur.AssignedAt)
			.IsRequired();

		builder.Property(ur => ur.IsActive)
			.IsRequired();

		// Indexes
		builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
			.IsUnique();

		builder.HasIndex(ur => ur.TenantId);

		builder.HasIndex(ur => ur.UserId);

		builder.HasIndex(ur => ur.RoleId);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(ur => ur.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne<User>()
			.WithMany()
			.HasForeignKey(ur => ur.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne<Role>()
			.WithMany()
			.HasForeignKey(ur => ur.RoleId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}