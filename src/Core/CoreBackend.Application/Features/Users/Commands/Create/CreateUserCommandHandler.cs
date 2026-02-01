using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Users.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserResponse>>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IRepositoryExtended<Role, Guid> _roleRepository;
	private readonly IRepositoryExtended<UserRole, Guid> _userRoleRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ICurrentUserService _currentUserService;
	private readonly IUnitOfWork _unitOfWork;

	public CreateUserCommandHandler(
		IRepositoryExtended<User, Guid> userRepository,
		IRepositoryExtended<Role, Guid> roleRepository,
		IRepositoryExtended<UserRole, Guid> userRoleRepository,
		IPasswordHasher passwordHasher,
		ICurrentUserService currentUserService,
		IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_roleRepository = roleRepository;
		_userRoleRepository = userRoleRepository;
		_passwordHasher = passwordHasher;
		_currentUserService = currentUserService;
		_unitOfWork = unitOfWork;
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
		var existingUser = await _userRepository.FirstOrDefaultAsync(
			u => u.Email == request.Email,
			cancellationToken);

		if (existingUser != null)
		{
			return Result.Failure<UserResponse>(
				Error.Create(ErrorCodes.User.AlreadyExists, "Email already exists."));
		}

		// Username benzersizlik kontrolü
		existingUser = await _userRepository.FirstOrDefaultAsync(
			u => u.Username == request.Username,
			cancellationToken);

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

		user.ConfirmEmail(); // Admin tarafından oluşturulduğu için

		await _userRepository.AddAsync(user, cancellationToken);

		// Rolleri ata
		var roleNames = new List<string>();
		if (request.RoleIds != null && request.RoleIds.Any())
		{
			foreach (var roleId in request.RoleIds)
			{
				var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
				if (role != null)
				{
					var userRole = UserRole.Create(tenantId.Value, user.Id, roleId);
					await _userRoleRepository.AddAsync(userRole, cancellationToken);
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