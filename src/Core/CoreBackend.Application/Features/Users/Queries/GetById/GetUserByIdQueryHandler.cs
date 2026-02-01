using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Queries.GetById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IRepositoryExtended<UserRole, Guid> _userRoleRepository;
	private readonly IRepositoryExtended<Role, Guid> _roleRepository;

	public GetUserByIdQueryHandler(
		IRepositoryExtended<User, Guid> userRepository,
		IRepositoryExtended<UserRole, Guid> userRoleRepository,
		IRepositoryExtended<Role, Guid> roleRepository)
	{
		_userRepository = userRepository;
		_userRoleRepository = userRoleRepository;
		_roleRepository = roleRepository;
	}

	public async Task<Result<UserResponse>> Handle(
		GetUserByIdQuery request,
		CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

		if (user == null)
		{
			return Result.Failure<UserResponse>(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		// Rolleri getir
		var userRoles = await _userRoleRepository.FindAsync(
			ur => ur.UserId == user.Id && ur.IsActive,
			cancellationToken);
		var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
		var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);

		return Result.Success(MapToResponse(user, roles.Select(r => r.Code).ToList()));
	}

	private static UserResponse MapToResponse(User user, List<string> roles)
	{
		return new UserResponse
		{
			Id = user.Id,
			TenantId = user.TenantId,
			Username = user.Username,
			Email = user.Email,
			FirstName = user.FirstName,
			LastName = user.LastName,
			FullName = user.FullName,
			Phone = user.Phone,
			Status = user.Status.ToString(),
			EmailConfirmed = user.EmailConfirmed,
			LastLoginAt = user.LastLoginAt,
			CreatedAt = user.CreatedAt,
			Roles = roles
		};
	}
}