using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
	private readonly ICurrentUserService _currentUserService;
	private readonly IUserSessionService _sessionService;
	private readonly ISessionHistoryService _historyService;
	private readonly IDeviceInfoService _deviceInfoService;

	public LogoutCommandHandler(
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

	public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
	{
		var sessionId = _currentUserService.SessionId;
		var userId = _currentUserService.UserId;
		var tenantId = _currentUserService.TenantId;

		if (string.IsNullOrEmpty(sessionId) || !userId.HasValue || !tenantId.HasValue)
		{
			return Result.Failure(Error.Create(ErrorCodes.Auth.Unauthorized, "Not authenticated."));
		}

		// Session'ı iptal et
		await _sessionService.RevokeSessionAsync(sessionId, cancellationToken);

		// Device bilgisini al
		var deviceInfo = await _deviceInfoService.GetDeviceInfoAsync(cancellationToken);

		// History'ye kaydet
		await _historyService.LogAsync(
			tenantId.Value,
			userId.Value,
			sessionId,
			SessionAction.Logout,
			deviceInfo,
			cancellationToken: cancellationToken);

		return Result.Success();
	}
}