using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");

		builder.HasKey(x => x.Id);

		// Soft Delete Query Filter
		builder.HasQueryFilter(x => !x.IsDeleted);

		builder.Property(x => x.Username)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.UsernameMaxLength);

		builder.Property(x => x.Email)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.EmailMaxLength);

		builder.Property(x => x.PasswordHash)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(x => x.FirstName)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.FirstNameMaxLength);

		builder.Property(x => x.LastName)
			.IsRequired()
			.HasMaxLength(EntityConstants.User.LastNameMaxLength);

		builder.Property(x => x.Phone)
			.HasMaxLength(EntityConstants.User.PhoneMaxLength);

		builder.Property(x => x.RefreshToken)
			.HasMaxLength(500);

		// 2FA Alanları
		builder.Property(x => x.TotpSecretKey)
			.HasMaxLength(EntityConstants.TwoFactor.TotpSecretKeyMaxLength);

		builder.Property(x => x.RecoveryCodes)
			.HasMaxLength(EntityConstants.TwoFactor.RecoveryCodesMaxLength);

		// Filtered Indexes
		builder.HasIndex(x => x.TenantId)
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Users_TenantId_Active");

		builder.HasIndex(x => new { x.TenantId, x.Email })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Users_TenantId_Email_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.Username })
			.IsUnique()
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Users_TenantId_Username_Unique_Active");

		builder.HasIndex(x => new { x.TenantId, x.Status })
			.HasFilter("\"IsDeleted\" = false")
			.HasDatabaseName("IX_Users_TenantId_Status_Active");

		// NOT: Tenant relationship TenantConfiguration'da tanımlı
		// UserRoles relationship burada tanımlanmıyor - UserRoleConfiguration'da
	}
}