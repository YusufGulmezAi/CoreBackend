using MediatR;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Refresh token command.
/// Access token'ı yeniler.
/// </summary>
public record RefreshTokenCommand(
	string AccessToken,
	string RefreshToken
) : IRequest<Result<AuthResponse>>;