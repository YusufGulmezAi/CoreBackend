using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Commands.LogoutAll;

public record LogoutAllCommand : IRequest<Result>;