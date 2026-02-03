using MediatR;
using CoreBackend.Application.Common.Interfaces;
using CoreBackend.Contracts.Auth.Responses;
using CoreBackend.Domain.Common.Primitives;
using CoreBackend.Domain.Entities;
using CoreBackend.Domain.Enums;
using CoreBackend.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace CoreBackend.Application.Features.Auth.Commands.EnableTwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result<TwoFactorSetupResponse>>
{
	private readonly ICurrentUserService _currentUserService;
	private readonly ITotpService _totpService;
	private readonly IEmailService _emailService;
	private readonly ISmsService _smsService;
	private readonly IUnitOfWork _unitOfWork;

	public EnableTwoFactorCommandHandler(
		ICurrentUserService currentUserService,
		ITotpService totpService,
		IEmailService emailService,
		ISmsService smsService,
		IUnitOfWork unitOfWork)
	{
		_currentUserService = currentUserService;
		_totpService = totpService;
		_emailService = emailService;
		_smsService = smsService;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<TwoFactorSetupResponse>> Handle(
		EnableTwoFactorCommand request,
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

		if (user.TwoFactorEnabled)
		{
			return Result.Failure<TwoFactorSetupResponse>(
				Error.Create(ErrorCodes.TwoFactor.AlreadyEnabled, "Two-factor authentication is already enabled."));
		}

		// Tenant'ın izin verdiği metodları kontrol et
		var tenant = await _unitOfWork.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId.Value, cancellationToken);
		if (tenant != null && tenant.AllowedTwoFactorMethods.Any() && !tenant.AllowedTwoFactorMethods.Contains(request.Method))
		{
			return Result.Failure<TwoFactorSetupResponse>(
				Error.Create(ErrorCodes.TwoFactor.MethodNotAllowed, "This two-factor method is not allowed by your organization."));
		}

		var response = new TwoFactorSetupResponse
		{
			Method = request.Method.ToString()
		};

		switch (request.Method)
		{
			case TwoFactorMethod.Totp:
				var secretKey = _totpService.GenerateSecretKey();
				var qrCodeUri = _totpService.GenerateQrCodeUri(user.Email, secretKey);
				response.SecretKey = secretKey;
				response.QrCodeUri = qrCodeUri;
				response.Message = "Scan the QR code with your authenticator app, then verify with the code.";
				break;

			case TwoFactorMethod.Email:
				var emailCode = TwoFactorCode.Create(tenantId.Value, userId.Value, TwoFactorMethod.Email);
				await _unitOfWork.TwoFactorCodes.AddAsync(emailCode, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				await _emailService.SendTwoFactorCodeAsync(user.Email, emailCode.Code, 5, cancellationToken);
				response.Message = "A verification code has been sent to your email.";
				break;

			case TwoFactorMethod.Sms:
				if (string.IsNullOrEmpty(user.Phone))
				{
					return Result.Failure<TwoFactorSetupResponse>(
						Error.Create(ErrorCodes.Sms.InvalidPhoneNumber, "Phone number is required for SMS verification."));
				}
				var smsCode = TwoFactorCode.Create(tenantId.Value, userId.Value, TwoFactorMethod.Sms);
				await _unitOfWork.TwoFactorCodes.AddAsync(smsCode, cancellationToken);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				await _smsService.SendTwoFactorCodeAsync(user.Phone, smsCode.Code, 5, cancellationToken);
				response.Message = "A verification code has been sent to your phone.";
				break;

			default:
				return Result.Failure<TwoFactorSetupResponse>(
					Error.Create(ErrorCodes.TwoFactor.MethodNotAllowed, "Invalid two-factor method."));
		}

		return Result.Success(response);
	}
}