using MediatR;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Features.Auth.Commands.VerifyTwoFactorSetup;

public record VerifyTwoFactorSetupCommand(
	string Code,
	TwoFactorMethod Method,
	string? SecretKey = null // TOTP için
) : IRequest<Result<TwoFactorSetupResponse>>;