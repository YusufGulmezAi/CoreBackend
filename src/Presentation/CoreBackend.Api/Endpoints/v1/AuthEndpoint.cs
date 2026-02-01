using Asp.Versioning;
using MediatR;
using CoreBackend.Application.Features.Auth.Commands.Login;
using CoreBackend.Application.Features.Auth.Commands.Register;
using CoreBackend.Application.Features.Auth.Commands.RefreshToken;
using CoreBackend.Application.Features.Auth.Commands.Logout;
using CoreBackend.Application.Features.Auth.Commands.LogoutAll;
using CoreBackend.Application.Features.Auth.Commands.EnableTwoFactor;
using CoreBackend.Application.Features.Auth.Commands.VerifyTwoFactorSetup;
using CoreBackend.Application.Features.Auth.Commands.DisableTwoFactor;
using CoreBackend.Application.Features.Auth.Queries.GetActiveSessions;
using CoreBackend.Contracts.Auth.Requests;
using CoreBackend.Contracts.Common;
using CoreBackend.Domain.Enums;

namespace CoreBackend.Api.Endpoints.v1;

public class AuthEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		var versionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1, 0))
			.Build();

		var group = app.MapGroup("/api/v{version:apiVersion}/auth")
			.WithApiVersionSet(versionSet)
			.WithTags("Auth");

		// Public endpoints
		group.MapPost("/login", Login).WithName("Login").AllowAnonymous();
		group.MapPost("/register", Register).WithName("Register").AllowAnonymous();
		group.MapPost("/refresh-token", RefreshToken).WithName("RefreshToken").AllowAnonymous();

		// Authenticated endpoints
		group.MapPost("/logout", Logout).WithName("Logout").RequireAuthorization();
		group.MapPost("/logout-all", LogoutAll).WithName("LogoutAll").RequireAuthorization();
		group.MapGet("/sessions", GetActiveSessions).WithName("GetActiveSessions").RequireAuthorization();

		// 2FA endpoints
		group.MapPost("/2fa/enable", EnableTwoFactor).WithName("EnableTwoFactor").RequireAuthorization();
		group.MapPost("/2fa/verify-setup", VerifyTwoFactorSetup).WithName("VerifyTwoFactorSetup").RequireAuthorization();
		group.MapPost("/2fa/disable", DisableTwoFactor).WithName("DisableTwoFactor").RequireAuthorization();
	}

	private static async Task<IResult> Login(
		LoginRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new LoginCommand(request.Email, request.Password, request.RememberMe);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Register(
		RegisterRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new RegisterCommand(
			request.TenantName, request.Username, request.Email,
			request.Password, request.ConfirmPassword,
			request.FirstName, request.LastName, request.Phone);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> RefreshToken(
		RefreshTokenRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Logout(
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new LogoutCommand();
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse.SuccessResponse("Logged out successfully."));
	}

	private static async Task<IResult> LogoutAll(
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new LogoutAllCommand();
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse.SuccessResponse("Logged out from all devices."));
	}

	private static async Task<IResult> GetActiveSessions(
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var query = new GetActiveSessionsQuery();
		var result = await mediator.Send(query, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> EnableTwoFactor(
		EnableTwoFactorRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		if (!Enum.TryParse<TwoFactorMethod>(request.Method, true, out var method))
		{
			return Results.BadRequest(ApiResponse<object>.FailureResponse("Invalid two-factor method.", "INVALID_METHOD"));
		}

		var command = new EnableTwoFactorCommand(method);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> VerifyTwoFactorSetup(
		VerifyTwoFactorSetupRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		if (!Enum.TryParse<TwoFactorMethod>(request.Method, true, out var method))
		{
			return Results.BadRequest(ApiResponse<object>.FailureResponse("Invalid two-factor method.", "INVALID_METHOD"));
		}

		var command = new VerifyTwoFactorSetupCommand(request.Code, method, request.SecretKey);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> DisableTwoFactor(
		DisableTwoFactorRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new DisableTwoFactorCommand(request.Password, request.Code);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse.SuccessResponse("Two-factor authentication disabled."));
	}
}