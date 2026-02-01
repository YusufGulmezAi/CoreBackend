using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Application.Features.Users.Queries.GetPaged;

public class GetUsersPagedQueryHandler
	: IRequestHandler<GetUsersPagedQuery, Result<PagedResponse<UserResponse>>>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IRepositoryExtended<UserRole, Guid> _userRoleRepository;
	private readonly IRepositoryExtended<Role, Guid> _roleRepository;

	public GetUsersPagedQueryHandler(
		IRepositoryExtended<User, Guid> userRepository,
		IRepositoryExtended<UserRole, Guid> userRoleRepository,
		IRepositoryExtended<Role, Guid> roleRepository)
	{
		_userRepository = userRepository;
		_userRoleRepository = userRoleRepository;
		_roleRepository = roleRepository;
	}

	public async Task<Result<PagedResponse<UserResponse>>> Handle(
		GetUsersPagedQuery request,
		CancellationToken cancellationToken)
	{
		var pagedRequest = new QueryOptions
		{
			PageNumber = request.PageNumber,
			PageSize = request.PageSize,
			SearchText = request.SearchText,
			SearchFields = new List<string> { "Username", "Email", "FirstName", "LastName" }
		};

		if (!string.IsNullOrEmpty(request.SortBy))
		{
			pagedRequest.Query = new DynamicQuery
			{
				Sort = new List<SortDescriptor>
				{
					new() { Field = request.SortBy, Direction = request.SortDescending ? SortDirection.Descending : SortDirection.Ascending }
				}
			};
		}

		var result = await _userRepository.GetPagedAsync(pagedRequest, cancellationToken);

		// Her kullanıcı için rolleri getir
		var userResponses = new List<UserResponse>();
		foreach (var user in result.Items)
		{
			var userRoles = await _userRoleRepository.FindAsync(
				ur => ur.UserId == user.Id && ur.IsActive,
				cancellationToken);
			var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
			var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);

			userResponses.Add(MapToResponse(user, roles.Select(r => r.Code).ToList()));
		}

		var response = new PagedResponse<UserResponse>
		{
			Items = userResponses,
			PageNumber = result.PageNumber,
			PageSize = result.PageSize,
			TotalCount = result.TotalCount
		};

		return Result.Success(response);
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