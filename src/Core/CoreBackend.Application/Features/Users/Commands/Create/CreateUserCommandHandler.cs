using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserResponse>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ICurrentUserService _currentUserService;

	public CreateUserCommandHandler(
		IUnitOfWork unitOfWork,
		IPasswordHasher passwordHasher,
		ICurrentUserService currentUserService)
	{
		_unitOfWork = unitOfWork;
		_passwordHasher = passwordHasher;
		_currentUserService = currentUserService;
	}

	public async Task<Result<UserResponse>> Handle(
		CreateUserCommand request,
		CancellationToken cancellationToken)
	{
		var tenantId = _currentUserService.TenantId;

		if (!tenantId.HasValue)
		{
			return Result.Failure<UserResponse>(
				Error.Create(ErrorCodes.Auth.Unauthorized, "Tenant not found."));
		}

		// Email benzersizlik kontrolü
		var existingUser = await _unitOfWork.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

		if (existingUser != null)
		{
			return Result.Failure<UserResponse>(
				Error.Create(ErrorCodes.User.AlreadyExists, "Email already exists."));
		}

		// Username benzersizlik kontrolü
		existingUser = await _unitOfWork.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

		if (existingUser != null)
		{
			return Result.Failure<UserResponse>(
				Error.Create(ErrorCodes.User.AlreadyExists, "Username already exists."));
		}

		var passwordHash = _passwordHasher.HashPassword(request.Password);

		var user = User.Create(
			tenantId.Value,
			request.Username,
			request.Email,
			passwordHash,
			request.FirstName,
			request.LastName,
			request.Phone);

		user.ConfirmEmail();

		await _unitOfWork.Users.AddAsync(user, cancellationToken);

		// Rolleri ata
		var roleNames = new List<string>();
		if (request.RoleIds != null && request.RoleIds.Any())
		{
			foreach (var roleId in request.RoleIds)
			{
				var role = await _unitOfWork.Roles
					.AsNoTracking()
					.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

				if (role != null)
				{
					var userRole = UserRole.Create(tenantId.Value, user.Id, roleId);
					await _unitOfWork.UserRoles.AddAsync(userRole, cancellationToken);
					roleNames.Add(role.Code);
				}
			}
		}

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success(MapToResponse(user, roleNames));
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