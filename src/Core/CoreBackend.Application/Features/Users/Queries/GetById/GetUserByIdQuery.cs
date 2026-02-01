using MediatR;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Users.Queries.GetById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;