using MediatR;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Users.Commands.Create;

public record CreateUserCommand(
	string Username,
	string Email,
	string Password,
	string FirstName,
	string LastName,
	string? Phone,
	List<Guid>? RoleIds
) : IRequest<Result<UserResponse>>;