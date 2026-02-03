using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Commands.LogoutAll;

public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, Result>
{
	private readonly ICurrentUserService _currentUserService;
	private readonly IUserSessionService _sessionService;
	private readonly ISessionHistoryService _historyService;
	private readonly IDeviceInfoService _deviceInfoService;

	public LogoutAllCommandHandler(
		ICurrentUserService currentUserService,
		IUserSessionService sessionService,
		ISessionHistoryService historyService,
		IDeviceInfoService deviceInfoService)
	{
		_currentUserService = currentUserService;
		_sessionService = sessionService;
		_historyService = historyService;
		_deviceInfoService = deviceInfoService;
	}

	public async Task<Result> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;
		var tenantId = _currentUserService.TenantId;
		var currentSessionId = _currentUserService.SessionId;

		if (!userId.HasValue || !tenantId.HasValue)
		{
			return Result.Failure(Error.Create(ErrorCodes.Auth.Unauthorized, "Not authenticated."));
		}

		// Tüm aktif session'ları al
		var activeSessions = await _sessionService.GetUserActiveSessionsAsync(userId.Value, cancellationToken);

		// Device bilgisini al
		var deviceInfo = await _deviceInfoService.GetDeviceInfoAsync(cancellationToken);

		// Her session için history kaydet ve iptal et
		foreach (var session in activeSessions)
		{
			// Sadece session ID'yi kullanarak history log'u oluştur
			// Session zaten UserSessionData içindeki bilgileri içeriyor
		}

		// Tüm session'ları iptal et
		await _sessionService.RevokeAllUserSessionsAsync(userId.Value, cancellationToken);

		// Tek bir LogoutAll kaydı
		await _historyService.LogAsync(
			tenantId.Value,
			userId.Value,
			currentSessionId ?? "all",
			SessionAction.LogoutAll,
			deviceInfo,
			cancellationToken: cancellationToken);

		return Result.Success();
	}
}