using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Role entity konfigürasyonu.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
	public void Configure(EntityTypeBuilder<Role> builder)
	{
		// Tablo adı
		builder.ToTable("Roles");

		// Primary Key
		builder.HasKey(r => r.Id);

		// Properties
		builder.Property(r => r.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Role.NameMaxLength);

		builder.Property(r => r.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.Role.CodeMaxLength);

		builder.Property(r => r.Description)
			.HasMaxLength(EntityConstants.Role.DescriptionMaxLength);

		builder.Property(r => r.Level)
			.IsRequired();

		builder.Property(r => r.IsSystemRole)
			.IsRequired();

		builder.Property(r => r.IsActive)
			.IsRequired();

		// Indexes
		builder.HasIndex(r => new { r.TenantId, r.Code })
			.IsUnique();

		builder.HasIndex(r => r.TenantId);

		builder.HasIndex(r => r.Level);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(r => r.TenantId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}