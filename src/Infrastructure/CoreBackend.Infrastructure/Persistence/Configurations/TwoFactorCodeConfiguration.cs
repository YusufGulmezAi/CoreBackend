using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class TwoFactorCodeConfiguration : IEntityTypeConfiguration<TwoFactorCode>
{
	public void Configure(EntityTypeBuilder<TwoFactorCode> builder)
	{
		builder.ToTable("TwoFactorCodes");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Code)
			.IsRequired()
			.HasMaxLength(EntityConstants.TwoFactor.CodeLength);

		builder.Property(x => x.Method)
			.IsRequired();

		builder.Property(x => x.ExpiresAt)
			.IsRequired();

		// Indexes
		builder.HasIndex(x => x.TenantId);
		builder.HasIndex(x => x.UserId);
		builder.HasIndex(x => new { x.UserId, x.Code, x.IsUsed });
		builder.HasIndex(x => x.ExpiresAt);

		// Relationships
		builder.HasOne(x => x.Tenant)
			.WithMany()
			.HasForeignKey(x => x.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.User)
			.WithMany()
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}