using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>;