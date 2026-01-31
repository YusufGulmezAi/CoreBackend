using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Tenant entity konfigürasyonu.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
	public void Configure(EntityTypeBuilder<Tenant> builder)
	{
		// Tablo adı
		builder.ToTable("Tenants");

		// Primary Key
		builder.HasKey(t => t.Id);

		// Properties
		builder.Property(t => t.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Tenant.NameMaxLength);

		builder.Property(t => t.Email)
			.IsRequired()
			.HasMaxLength(EntityConstants.Tenant.EmailMaxLength);

		builder.Property(t => t.Phone)
			.HasMaxLength(EntityConstants.Tenant.PhoneMaxLength);

		builder.Property(t => t.Status)
			.IsRequired();

		builder.Property(t => t.Settings)
			.HasMaxLength(EntityConstants.Common.SettingsMaxLength);

		// Indexes
		builder.HasIndex(t => t.Email)
			.IsUnique();

		builder.HasIndex(t => t.Status);
	}
}