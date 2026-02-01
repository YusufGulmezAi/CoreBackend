using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Infrastructure.Persistence.Context;

namespace CoreBackend.Infrastructure.Services;

/// <summary>
/// Session history servis implementasyonu.
/// </summary>
public class SessionHistoryService : ISessionHistoryService
{
	private readonly ApplicationDbContext _context;

	public SessionHistoryService(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task LogAsync(
		Guid tenantId,
		Guid userId,
		string sessionId,
		SessionAction action,
		DeviceInfo? deviceInfo = null,
		Guid? revokedByUserId = null,
		string? revokeReason = null,
		CancellationToken cancellationToken = default)
	{
		var history = SessionHistory.Create(
			tenantId,
			userId,
			sessionId,
			action,
			deviceInfo?.IpAddress,
			deviceInfo?.UserAgent,
			deviceInfo?.BrowserName,
			deviceInfo?.OperatingSystem,
			deviceInfo?.DeviceType,
			deviceInfo?.GeoLocation?.ToString(),
			deviceInfo?.GeoLocation?.Country,
			deviceInfo?.GeoLocation?.City);

		if (revokedByUserId.HasValue)
		{
			history.SetRevokedBy(revokedByUserId.Value, revokeReason);
		}

		await _context.SessionHistories.AddAsync(history, cancellationToken);
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<SessionHistoryDto>> GetUserHistoryAsync(
		Guid userId,
		int pageNumber = 1,
		int pageSize = 20,
		CancellationToken cancellationToken = default)
	{
		var skip = (pageNumber - 1) * pageSize;

		var histories = await _context.SessionHistories
			.AsNoTracking()
			.Where(h => h.UserId == userId)
			.OrderByDescending(h => h.CreatedAt)
			.Skip(skip)
			.Take(pageSize)
			.Select(h => new SessionHistoryDto
			{
				Id = h.Id,
				UserId = h.UserId,
				SessionId = h.SessionId,
				Action = h.Action,
				IpAddress = h.IpAddress,
				BrowserName = h.BrowserName,
				OperatingSystem = h.OperatingSystem,
				DeviceType = h.DeviceType,
				Country = h.Country,
				City = h.City,
				CreatedAt = h.CreatedAt,
				RevokedByUserId = h.RevokedByUserId,
				RevokeReason = h.RevokeReason
			})
			.ToListAsync(cancellationToken);

		return histories;
	}

	public async Task<IReadOnlyList<SessionHistoryDto>> GetTenantHistoryAsync(
		Guid tenantId,
		int pageNumber = 1,
		int pageSize = 50,
		CancellationToken cancellationToken = default)
	{
		var skip = (pageNumber - 1) * pageSize;

		var histories = await _context.SessionHistories
			.AsNoTracking()
			.Include(h => h.User)
			.Include(h => h.RevokedByUser)
			.Where(h => h.TenantId == tenantId)
			.OrderByDescending(h => h.CreatedAt)
			.Skip(skip)
			.Take(pageSize)
			.Select(h => new SessionHistoryDto
			{
				Id = h.Id,
				UserId = h.UserId,
				UserEmail = h.User.Email,
				UserFullName = h.User.FullName,
				SessionId = h.SessionId,
				Action = h.Action,
				IpAddress = h.IpAddress,
				BrowserName = h.BrowserName,
				OperatingSystem = h.OperatingSystem,
				DeviceType = h.DeviceType,
				Country = h.Country,
				City = h.City,
				CreatedAt = h.CreatedAt,
				RevokedByUserId = h.RevokedByUserId,
				RevokedByUserName = h.RevokedByUser != null ? h.RevokedByUser.FullName : null,
				RevokeReason = h.RevokeReason
			})
			.ToListAsync(cancellationToken);

		return histories;
	}
}