using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Permission entity konfigürasyonu.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
	public void Configure(EntityTypeBuilder<Permission> builder)
	{
		// Tablo adı
		builder.ToTable("Permissions");

		// Primary Key
		builder.HasKey(p => p.Id);

		// Properties
		builder.Property(p => p.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Permission.NameMaxLength);

		builder.Property(p => p.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.Permission.CodeMaxLength);

		builder.Property(p => p.Description)
			.HasMaxLength(EntityConstants.Permission.DescriptionMaxLength);

		builder.Property(p => p.Group)
			.IsRequired()
			.HasMaxLength(EntityConstants.Permission.GroupMaxLength);

		builder.Property(p => p.IsActive)
			.IsRequired();

		// Indexes
		builder.HasIndex(p => p.Code)
			.IsUnique();

		builder.HasIndex(p => p.Group);
	}
}