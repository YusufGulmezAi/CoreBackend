using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Application.Common.Models;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;

namespace CoreBackend.Application.Features.Users.Queries.GetPaged;

public class GetUsersPagedQueryHandler
	: IRequestHandler<GetUsersPagedQuery, Result<PaginatedList<UserResponse>>>
{
	private readonly IUnitOfWork _unitOfWork;

	public GetUsersPagedQueryHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<PaginatedList<UserResponse>>> Handle(
		GetUsersPagedQuery request,
		CancellationToken cancellationToken)
	{
		var queryOptions = new QueryOptions
		{
			PageNumber = request.PageNumber,
			PageSize = request.PageSize,
			SearchText = request.SearchText,
			SearchFields = new List<string> { "Username", "Email", "FirstName", "LastName" }
		};

		if (!string.IsNullOrEmpty(request.SortBy))
		{
			queryOptions.Query = new DynamicQuery
			{
				Sort = new List<SortDescriptor>
				{
					new()
					{
						Field = request.SortBy,
						Direction = request.SortDescending
							? SortDirection.Descending
							: SortDirection.Ascending
					}
				}
			};
		}

		var query = _unitOfWork.QueryNoTracking<User>();
		var result = await _unitOfWork.GetPagedAsync(query, queryOptions, cancellationToken);

		// Her kullanıcı için rolleri getir
		var userResponses = new List<UserResponse>();
		foreach (var user in result.Items)
		{
			var roleIds = await _unitOfWork.UserRoles
				.AsNoTracking()
				.Where(ur => ur.UserId == user.Id && ur.IsActive)
				.Select(ur => ur.RoleId)
				.ToListAsync(cancellationToken);

			var roles = await _unitOfWork.Roles
				.AsNoTracking()
				.Where(r => roleIds.Contains(r.Id))
				.Select(r => r.Code)
				.ToListAsync(cancellationToken);

			userResponses.Add(MapToResponse(user, roles));
		}

		var response = new PaginatedList<UserResponse>
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