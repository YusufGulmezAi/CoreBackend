using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Infrastructure.Persistence.Configurations;

public class SessionHistoryConfiguration : IEntityTypeConfiguration<SessionHistory>
{
	public void Configure(EntityTypeBuilder<SessionHistory> builder)
	{
		builder.ToTable("SessionHistories");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.SessionId)
			.IsRequired()
			.HasMaxLength(EntityConstants.SessionHistory.SessionIdMaxLength);

		builder.Property(x => x.Action)
			.IsRequired();

		builder.Property(x => x.IpAddress)
			.HasMaxLength(EntityConstants.SessionHistory.IpAddressMaxLength);

		builder.Property(x => x.UserAgent)
			.HasMaxLength(EntityConstants.SessionHistory.UserAgentMaxLength);

		builder.Property(x => x.BrowserName)
			.HasMaxLength(EntityConstants.SessionHistory.BrowserNameMaxLength);

		builder.Property(x => x.OperatingSystem)
			.HasMaxLength(EntityConstants.SessionHistory.OperatingSystemMaxLength);

		builder.Property(x => x.DeviceType)
			.HasMaxLength(EntityConstants.SessionHistory.DeviceTypeMaxLength);

		builder.Property(x => x.GeoLocation)
			.HasMaxLength(EntityConstants.SessionHistory.GeoLocationMaxLength);

		builder.Property(x => x.Country)
			.HasMaxLength(EntityConstants.SessionHistory.CountryMaxLength);

		builder.Property(x => x.City)
			.HasMaxLength(EntityConstants.SessionHistory.CityMaxLength);

		builder.Property(x => x.RevokeReason)
			.HasMaxLength(EntityConstants.SessionHistory.RevokeReasonMaxLength);

		builder.Property(x => x.AdditionalData)
			.HasMaxLength(EntityConstants.SessionHistory.AdditionalDataMaxLength);

		// Indexes
		builder.HasIndex(x => x.TenantId);
		builder.HasIndex(x => x.UserId);
		builder.HasIndex(x => x.SessionId);
		builder.HasIndex(x => x.Action);
		builder.HasIndex(x => x.CreatedAt);
		builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
		builder.HasIndex(x => new { x.UserId, x.CreatedAt });

		// Relationships - Navigation property kullanarak tanımla
		builder.HasOne(x => x.Tenant)
			.WithMany()
			.HasForeignKey(x => x.TenantId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.User)
			.WithMany()
			.HasForeignKey(x => x.UserId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(x => x.RevokedByUser)
			.WithMany()
			.HasForeignKey(x => x.RevokedByUserId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}