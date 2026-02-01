using MediatR;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Users.Commands.Delete;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;