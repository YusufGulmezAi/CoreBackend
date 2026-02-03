using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.Update;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserResponse>>
{
	private readonly IUnitOfWork _unitOfWork;

	public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<UserResponse>> Handle(
		UpdateUserCommand request,
		CancellationToken cancellationToken)
	{
		var user = await _unitOfWork.Users
			.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

		if (user == null)
		{
			return Result.Failure<UserResponse>(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		user.UpdateProfile(
			request.FirstName ?? user.FirstName,
			request.LastName ?? user.LastName,
			request.Phone);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// Rolleri getir
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

		return Result.Success(MapToResponse(user, roles));
	}

	private static UserResponse MapToResponse(Domain.Entities.User user, List<string> roles)
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