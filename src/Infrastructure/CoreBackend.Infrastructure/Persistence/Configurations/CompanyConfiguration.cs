using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
	public void Configure(EntityTypeBuilder<Company> builder)
	{
		builder.ToTable("Companies");

		builder.HasKey(x => x.Id);

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(EntityConstants.Company.NameMaxLength);

		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.Company.CodeMaxLength);

		builder.Property(x => x.TaxNumber)
			.HasMaxLength(EntityConstants.Company.TaxNumberMaxLength);

		builder.Property(x => x.Address)
			.HasMaxLength(EntityConstants.Company.AddressMaxLength);

		builder.Property(x => x.Phone)
			.HasMaxLength(EntityConstants.Company.PhoneMaxLength);

		builder.Property(x => x.Email)
			.HasMaxLength(EntityConstants.Company.EmailMaxLength);

		// Filtered Indexes
		builder.HasIndex(x => x.TenantId)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Companies_TenantId_Active");

		builder.HasIndex(x => new { x.TenantId, x.Code })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Companies_TenantId_Code_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.Status })
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Companies_TenantId_Status_Active");

		// NOT: Tenant relationship TenantConfiguration'da tanımlı
	}
}