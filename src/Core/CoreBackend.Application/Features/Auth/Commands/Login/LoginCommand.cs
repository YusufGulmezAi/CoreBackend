using MediatR;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Login command.
/// </summary>
public record LoginCommand(
	string Email,
	string Password,
	bool RememberMe = false
) : IRequest<Result<AuthResponse>>;