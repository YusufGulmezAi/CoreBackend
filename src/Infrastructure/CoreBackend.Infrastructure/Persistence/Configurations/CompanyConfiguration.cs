using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Constants;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

/// <summary>
/// Company entity konfigürasyonu.
/// </summary>
public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
	public void Configure(EntityTypeBuilder<Company> builder)
	{
		// Tablo adı
		builder.ToTable("Companies");

		// Primary Key
		builder.HasKey(c => c.Id);

		// Properties
		builder.Property(c => c.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Company.NameMaxLength);

		builder.Property(c => c.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.Company.CodeMaxLength);

		builder.Property(c => c.TaxNumber)
			.HasMaxLength(EntityConstants.Company.TaxNumberMaxLength);

		builder.Property(c => c.TaxOffice)
			.HasMaxLength(EntityConstants.Company.TaxOfficeMaxLength);

		builder.Property(c => c.Email)
			.HasMaxLength(EntityConstants.Company.EmailMaxLength);

		builder.Property(c => c.Phone)
			.HasMaxLength(EntityConstants.Company.PhoneMaxLength);

		builder.Property(c => c.Address)
			.HasMaxLength(EntityConstants.Company.AddressMaxLength);

		builder.Property(c => c.Status)
			.IsRequired();

		builder.Property(c => c.Settings)
			.HasMaxLength(EntityConstants.Common.SettingsMaxLength);

		// Indexes
		builder.HasIndex(c => new { c.TenantId, c.Code })
			.IsUnique();

		builder.HasIndex(c => c.TenantId);

		builder.HasIndex(c => c.Status);

		// Relationships
		builder.HasOne<Tenant>()
			.WithMany()
			.HasForeignKey(c => c.TenantId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}