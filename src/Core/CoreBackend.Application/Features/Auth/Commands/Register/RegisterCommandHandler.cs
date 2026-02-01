using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Register command handler.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
	private readonly IRepositoryExtended<Tenant, Guid> _tenantRepository;
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IRepositoryExtended<Role, Guid> _roleRepository;
	private readonly IRepositoryExtended<UserRole, Guid> _userRoleRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IUnitOfWork _unitOfWork;

	public RegisterCommandHandler(
		IRepositoryExtended<Tenant, Guid> tenantRepository,
		IRepositoryExtended<User, Guid> userRepository,
		IRepositoryExtended<Role, Guid> roleRepository,
		IRepositoryExtended<UserRole, Guid> userRoleRepository,
		IPasswordHasher passwordHasher,
		IUnitOfWork unitOfWork)
	{
		_tenantRepository = tenantRepository;
		_userRepository = userRepository;
		_roleRepository = roleRepository;
		_userRoleRepository = userRoleRepository;
		_passwordHasher = passwordHasher;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<RegisterResponse>> Handle(
		RegisterCommand request,
		CancellationToken cancellationToken)
	{
		// 1. Email benzersizlik kontrolü (global)
		var existingUser = await _userRepository.FirstOrDefaultIgnoreFiltersAsync(
			u => u.Email == request.Email,
			cancellationToken);

		if (existingUser != null)
		{
			return Result.Failure<RegisterResponse>(
				Error.Create(ErrorCodes.Auth.UserNotFound, "Email is already registered."));
		}

		// 2. Tenant oluştur
		var tenant = Tenant.Create(
			request.TenantName,
			request.Email,
			request.Phone);

		await _tenantRepository.AddAsync(tenant, cancellationToken);

		// 3. Varsayılan TenantAdmin rolü oluştur
		var adminRole = Role.Create(
			tenant.Id,
			"Tenant Admin",
			"TenantAdmin",
			RoleLevel.Tenant,
			"Full access to tenant resources",
			isSystemRole: true);

		await _roleRepository.AddAsync(adminRole, cancellationToken);

		// 4. Kullanıcı oluştur
		var passwordHash = _passwordHasher.HashPassword(request.Password);

		var user = User.Create(
			tenant.Id,
			request.Username,
			request.Email,
			passwordHash,
			request.FirstName,
			request.LastName,
			request.Phone);

		// Email onayını atla (geliştirme için)
		user.ConfirmEmail();

		await _userRepository.AddAsync(user, cancellationToken);

		// 5. Kullanıcıya TenantAdmin rolü ata
		var userRole = UserRole.Create(
			tenant.Id,
			user.Id,
			adminRole.Id);

		await _userRoleRepository.AddAsync(userRole, cancellationToken);

		// 6. Kaydet
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 7. Response oluştur
		var response = new RegisterResponse
		{
			TenantId = tenant.Id,
			UserId = user.Id,
			Email = user.Email,
			Message = "Registration successful. You can now login.",
			RequiresEmailConfirmation = false
		};

		return Result.Success(response);
	}
}