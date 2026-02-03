using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IPasswordHasher _passwordHasher;

	public RegisterCommandHandler(
		IUnitOfWork unitOfWork,
		IPasswordHasher passwordHasher)
	{
		_unitOfWork = unitOfWork;
		_passwordHasher = passwordHasher;
	}

	public async Task<Result<RegisterResponse>> Handle(
		RegisterCommand request,
		CancellationToken cancellationToken)
	{
		// 1. Email benzersizlik kontrolü (global)
		var existingUser = await _unitOfWork.QueryIgnoreFilters<User>()
			.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

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

		await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);

		// 3. Varsayılan TenantAdmin rolü oluştur
		var adminRole = Role.Create(
			tenant.Id,
			"Tenant Admin",
			"TenantAdmin",
			RoleLevel.Tenant,
			"Full access to tenant resources",
			isSystemRole: true);

		await _unitOfWork.Roles.AddAsync(adminRole, cancellationToken);

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

		user.ConfirmEmail();

		await _unitOfWork.Users.AddAsync(user, cancellationToken);

		// 5. Kullanıcıya TenantAdmin rolü ata
		var userRole = UserRole.Create(tenant.Id, user.Id, adminRole.Id);

		await _unitOfWork.UserRoles.AddAsync(userRole, cancellationToken);

		// 6. Kaydet
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// 7. Response
		return Result.Success(new RegisterResponse
		{
			TenantId = tenant.Id,
			UserId = user.Id,
			Email = user.Email,
			Message = "Registration successful. You can now login.",
			RequiresEmailConfirmation = false
		});
	}
}