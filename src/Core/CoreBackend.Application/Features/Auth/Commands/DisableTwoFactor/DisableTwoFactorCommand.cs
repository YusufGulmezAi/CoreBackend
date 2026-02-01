using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Commands.DisableTwoFactor;

public record DisableTwoFactorCommand(
	string Password,
	string? Code = null
) : IRequest<Result>;