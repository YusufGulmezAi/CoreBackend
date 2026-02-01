using MediatR;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Features.Auth.Commands.EnableTwoFactor;

public record EnableTwoFactorCommand(TwoFactorMethod Method) : IRequest<Result<TwoFactorSetupResponse>>;