using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;

namespace CoreBackend.Application.Features.Auth.Commands.DisableTwoFactor;

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, Result>
{
	//private readonly IRepositoryExtended<User, Guid> _userRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ITotpService _totpService;
	private readonly IUnitOfWork _unitOfWork;

	public DisableTwoFactorCommandHandler(
		//IRepositoryExtended<User, Guid> userRepository,
		ICurrentUserService currentUserService,
		IPasswordHasher passwordHasher,
		ITotpService totpService,
		IUnitOfWork unitOfWork)
	{
		//_userRepository = userRepository;
		_currentUserService = currentUserService;
		_passwordHasher = passwordHasher;
		_totpService = totpService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (!userId.HasValue)
		{
			return Result.Failure(Error.Create(ErrorCodes.Auth.Unauthorized, "Not authenticated."));
		}

		var user = await _unitOfWork.Users.FindAsync(userId.Value, cancellationToken);

		if (user == null)
		{
			return Result.Failure(Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		// Şifreyi doğrula
		if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
		{
			return Result.Failure(Error.Create(ErrorCodes.Auth.InvalidCredentials, "Invalid password."));
		}

		// 2FA aktifse kod doğrula
		if (user.TwoFactorEnabled && !string.IsNullOrEmpty(request.Code))
		{
			bool isValid = false;

			if (user.TwoFactorMethod == TwoFactorMethod.Totp && !string.IsNullOrEmpty(user.TotpSecretKey))
			{
				isValid = _totpService.VerifyCode(user.TotpSecretKey, request.Code);
			}

			if (!isValid)
			{
				return Result.Failure(Error.Create(ErrorCodes.TwoFactor.InvalidCode, "Invalid verification code."));
			}
		}

		user.DisableTwoFactor();
		_unitOfWork.Users.Update(user);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}