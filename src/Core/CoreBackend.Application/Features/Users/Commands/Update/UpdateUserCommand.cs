using MediatR;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Users.Commands.Update;

public record UpdateUserCommand(
	Guid Id,
	string? FirstName,
	string? LastName,
	string? Phone
) : IRequest<Result<UserResponse>>;