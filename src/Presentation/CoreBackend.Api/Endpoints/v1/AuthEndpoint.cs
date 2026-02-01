using Asp.Versioning;
using MediatR;
using CoreBackend.Application.Features.Auth.Commands.Login;
using CoreBackend.Application.Features.Auth.Commands.Register;
using CoreBackend.Application.Features.Auth.Commands.RefreshToken;
using CoreBackend.Contracts.Auth.Requests;
using CoreBackend.Contracts.Common;

namespace CoreBackend.Api.Endpoints.v1;

/// <summary>
/// Authentication endpoints.
/// </summary>
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

		group.MapPost("/login", Login)
			.WithName("Login")
			.AllowAnonymous();

		group.MapPost("/register", Register)
			.WithName("Register")
			.AllowAnonymous();

		group.MapPost("/refresh-token", RefreshToken)
			.WithName("RefreshToken")
			.AllowAnonymous();
	}

	/// <summary>
	/// Kullanıcı girişi.
	/// </summary>
	private static async Task<IResult> Login(
		LoginRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new LoginCommand(
			request.Email,
			request.Password,
			request.RememberMe);

		var result = await mediator.Send(command, cancellationToken);

		if (result.IsFailure)
		{
			return Results.BadRequest(ApiResponse<object>.FailureResponse(
				result.Error.Message,
				result.Error.Code));
		}

		return Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	/// <summary>
	/// Yeni tenant ve kullanıcı kaydı.
	/// </summary>
	private static async Task<IResult> Register(
		RegisterRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new RegisterCommand(
			request.TenantName,
			request.Username,
			request.Email,
			request.Password,
			request.ConfirmPassword,
			request.FirstName,
			request.LastName,
			request.Phone);

		var result = await mediator.Send(command, cancellationToken);

		if (result.IsFailure)
		{
			return Results.BadRequest(ApiResponse<object>.FailureResponse(
				result.Error.Message,
				result.Error.Code));
		}

		return Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	/// <summary>
	/// Token yenileme.
	/// </summary>
	private static async Task<IResult> RefreshToken(
		RefreshTokenRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new RefreshTokenCommand(
			request.AccessToken,
			request.RefreshToken);

		var result = await mediator.Send(command, cancellationToken);

		if (result.IsFailure)
		{
			return Results.BadRequest(ApiResponse<object>.FailureResponse(
				result.Error.Message,
				result.Error.Code));
		}

		return Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}
}