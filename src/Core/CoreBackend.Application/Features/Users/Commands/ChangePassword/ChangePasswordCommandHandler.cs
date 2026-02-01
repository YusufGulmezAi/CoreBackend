using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
	private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ICurrentUserService _currentUserService;
	private readonly IUnitOfWork _unitOfWork;

	public ChangePasswordCommandHandler(
		IRepositoryExtended<User, Guid> userRepository,
		IPasswordHasher passwordHasher,
		ICurrentUserService currentUserService,
		IUnitOfWork unitOfWork)
	{
		_userRepository = userRepository;
		_passwordHasher = passwordHasher;
		_currentUserService = currentUserService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(
		ChangePasswordCommand request,
		CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (!userId.HasValue)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Auth.UnauthorizedAccess, "User not authenticated."));
		}

		var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

		if (user == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		// Mevcut şifreyi doğrula
		if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Auth.InvalidCredentials, "Current password is incorrect."));
		}

		var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
		user.ChangePassword(newPasswordHash);

		_userRepository.Update(user);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}