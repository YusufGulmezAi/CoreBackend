using MediatR;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Queries.GetActiveSessions;

public record GetActiveSessionsQuery : IRequest<Result<List<ActiveSessionResponse>>>;