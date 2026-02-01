using Asp.Versioning;
using MediatR;
using CoreBackend.Application.Features.Users.Commands.Create;
using CoreBackend.Application.Features.Users.Commands.Update;
using CoreBackend.Application.Features.Users.Commands.Delete;
using CoreBackend.Application.Features.Users.Commands.ChangePassword;
using CoreBackend.Application.Features.Users.Queries.GetById;
using CoreBackend.Application.Features.Users.Queries.GetPaged;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Users.Requests;

namespace CoreBackend.Api.Endpoints.v1;

public class UserEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		var versionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1, 0))
			.Build();

		var group = app.MapGroup("/api/v{version:apiVersion}/users")
			.WithApiVersionSet(versionSet)
			.WithTags("Users")
			.RequireAuthorization();

		group.MapGet("", GetPaged).WithName("GetUsers");
		group.MapGet("/{id:guid}", GetById).WithName("GetUserById");
		group.MapPost("", Create).WithName("CreateUser");
		group.MapPut("/{id:guid}", Update).WithName("UpdateUser");
		group.MapDelete("/{id:guid}", Delete).WithName("DeleteUser");
		group.MapPost("/change-password", ChangePassword).WithName("ChangePassword");
	}

	private static async Task<IResult> GetPaged(
		int pageNumber,
		int pageSize,
		string? search,
		string? sortBy,
		bool sortDesc,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var query = new GetUsersPagedQuery(pageNumber, pageSize, search, sortBy, sortDesc);
		var result = await mediator.Send(query, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> GetById(
		Guid id,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var query = new GetUserByIdQuery(id);
		var result = await mediator.Send(query, cancellationToken);

		return result.IsFailure
			? Results.NotFound(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Create(
		CreateUserRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new CreateUserCommand(
			request.Username,
			request.Email,
			request.Password,
			request.FirstName,
			request.LastName,
			request.Phone,
			request.RoleIds);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Created($"/api/v1/users/{result.Value.Id}", ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Update(
		Guid id,
		UpdateUserRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new UpdateUserCommand(
			id,
			request.FirstName,
			request.LastName,
			request.Phone);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Delete(
		Guid id,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new DeleteUserCommand(id);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.NoContent();
	}

	private static async Task<IResult> ChangePassword(
		ChangePasswordRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new ChangePasswordCommand(
			request.CurrentPassword,
			request.NewPassword,
			request.ConfirmNewPassword);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse.SuccessResponse("Password changed successfully."));
	}
}