using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Queries.GetActiveSessions;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<ActiveSessionResponse>>>
{
	private readonly ICurrentUserService _currentUserService;
	private readonly IUserSessionService _sessionService;

	public GetActiveSessionsQueryHandler(
		ICurrentUserService currentUserService,
		IUserSessionService sessionService)
	{
		_currentUserService = currentUserService;
		_sessionService = sessionService;
	}

	public async Task<Result<List<ActiveSessionResponse>>> Handle(
		GetActiveSessionsQuery request,
		CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;
		var currentSessionId = _currentUserService.SessionId;

		if (!userId.HasValue)
		{
			return Result.Failure<List<ActiveSessionResponse>>(
				Error.Create(ErrorCodes.Auth.UnauthorizedAccess, "Not authenticated."));
		}

		var sessions = await _sessionService.GetUserActiveSessionsAsync(userId.Value, cancellationToken);

		var response = sessions.Select(s => new ActiveSessionResponse
		{
			SessionId = s.UserId.ToString(), // Session'dan sessionId almak gerekecek
			IpAddress = s.IpAddress,
			BrowserName = null, // UserSessionData'ya bu alanları eklemek gerekecek
			OperatingSystem = null,
			DeviceType = null,
			Country = null,
			City = null,
			CreatedAt = s.CreatedAt,
			LastActivityAt = s.LastActivityAt,
			ExpiresAt = s.ExpiresAt,
			IsCurrent = false // currentSessionId ile karşılaştır
		}).ToList();

		return Result.Success(response);
	}
}