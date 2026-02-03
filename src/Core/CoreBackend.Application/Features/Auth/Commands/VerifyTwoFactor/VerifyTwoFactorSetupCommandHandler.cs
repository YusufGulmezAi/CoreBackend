using System.Text.Json;
using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Constants;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace CoreBackend.Application.Features.Auth.Commands.VerifyTwoFactorSetup;

public class VerifyTwoFactorSetupCommandHandler : IRequestHandler<VerifyTwoFactorSetupCommand, Result<TwoFactorSetupResponse>>
{
	//private readonly IRepositoryExtended<User, Guid> _userRepository;
	//private readonly IRepositoryExtended<TwoFactorCode, Guid> _twoFactorCodeRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly ITotpService _totpService;
	private readonly IUnitOfWork _unitOfWork;

	public VerifyTwoFactorSetupCommandHandler(
		//IRepositoryExtended<User, Guid> userRepository,
		//IRepositoryExtended<TwoFactorCode, Guid> twoFactorCodeRepository,
		ICurrentUserService currentUserService,
		ITotpService totpService,
		IUnitOfWork unitOfWork)
	{
		//_userRepository = userRepository;
		//_twoFactorCodeRepository = twoFactorCodeRepository;
		_currentUserService = currentUserService;
		_totpService = totpService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TwoFactorSetupResponse>> Handle(
		VerifyTwoFactorSetupCommand request,
		CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;
		var tenantId = _currentUserService.TenantId;

		if (!userId.HasValue || !tenantId.HasValue)
		{
			return Result.Failure<TwoFactorSetupResponse>(
				Error.Create(ErrorCodes.Auth.Unauthorized, "Not authenticated."));
		}

		var user = await _unitOfWork.Users.FindAsync(userId.Value, cancellationToken);
		if (user == null)
		{
			return Result.Failure<TwoFactorSetupResponse>(
				Error.Create(ErrorCodes.User.NotFound, "User not found."));
		}

		bool isValid = false;

		switch (request.Method)
		{
			case TwoFactorMethod.Totp:
				if (string.IsNullOrEmpty(request.SecretKey))
				{
					return Result.Failure<TwoFactorSetupResponse>(
						Error.Create(ErrorCodes.TwoFactor.SetupRequired, "Secret key is required."));
				}
				isValid = _totpService.VerifyCode(request.SecretKey, request.Code);
				break;

			case TwoFactorMethod.Email:
			case TwoFactorMethod.Sms:
				var twoFactorCode = await _unitOfWork.TwoFactorCodes.FirstOrDefaultAsync(
					c => c.UserId == userId.Value && c.Method == request.Method && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow,
					cancellationToken);

				if (twoFactorCode == null)
				{
					return Result.Failure<TwoFactorSetupResponse>(
						Error.Create(ErrorCodes.TwoFactor.CodeExpired, "Verification code expired or not found."));
				}

				isValid = twoFactorCode.Verify(request.Code);
				_unitOfWork.TwoFactorCodes.Update(twoFactorCode);
				break;
		}

		if (!isValid)
		{
			return Result.Failure<TwoFactorSetupResponse>(
				Error.Create(ErrorCodes.TwoFactor.InvalidCode, "Invalid verification code."));
		}

		// 2FA'yı aktifleştir
		user.EnableTwoFactor(request.Method, request.Method == TwoFactorMethod.Totp ? request.SecretKey : null);

		// Recovery kodları oluştur
		var recoveryCodes = GenerateRecoveryCodes(EntityConstants.TwoFactor.RecoveryCodeCount);
		user.SetRecoveryCodes(JsonSerializer.Serialize(recoveryCodes), recoveryCodes.Count);
		_unitOfWork.Users.Update(user);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success(new TwoFactorSetupResponse
		{
			Method = request.Method.ToString(),
			RecoveryCodes = recoveryCodes,
			Message = "Two-factor authentication has been enabled. Save your recovery codes in a safe place."
		});
	}

	private static List<string> GenerateRecoveryCodes(int count)
	{
		var codes = new List<string>();
		var random = new Random();

		for (int i = 0; i < count; i++)
		{
			var code = $"{random.Next(10000, 99999)}-{random.Next(10000, 99999)}";
			codes.Add(code);
		}

		return codes;
	}
}