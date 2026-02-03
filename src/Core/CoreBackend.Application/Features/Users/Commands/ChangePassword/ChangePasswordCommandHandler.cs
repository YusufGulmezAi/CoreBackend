using MediatR;
using Microsoft.EntityFrameworkCore;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ICurrentUserService _currentUserService;

	public ChangePasswordCommandHandler(
		IUnitOfWork unitOfWork,
		IPasswordHasher passwordHasher,
		ICurrentUserService currentUserService)
	{
		_unitOfWork = unitOfWork;
		_passwordHasher = passwordHasher;
		_currentUserService = currentUserService;
	}

	public async Task<Result> Handle(
		ChangePasswordCommand request,
		CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (!userId.HasValue)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Auth.Unauthorized, "User not authenticated."));
		}

		var user = await _unitOfWork.Users
			.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

		if (user == null)
		{
			return Result.Failure(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
		{
			return Result.Failure(
				Error.Create(ErrorCodes.Auth.InvalidCredentials, "Current password is incorrect."));
		}

		var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
		user.ChangePassword(newPasswordHash);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}