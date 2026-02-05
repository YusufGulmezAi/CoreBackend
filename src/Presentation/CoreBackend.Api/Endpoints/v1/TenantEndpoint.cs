using Asp.Versioning;
using MediatR;
using CoreBackend.Application.Features.Tenants.Commands.Create;
using CoreBackend.Application.Features.Tenants.Commands.Update;
using CoreBackend.Application.Features.Tenants.Commands.Delete;
using CoreBackend.Application.Features.Tenants.Queries.GetById;
using CoreBackend.Application.Features.Tenants.Queries.GetPaged;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Tenants.Requests;
using CoreBackend.Contracts.Tenants.Responses;

namespace CoreBackend.Api.Endpoints.v1;

public class TenantEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		var versionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1, 0))
			.Build();

		var group = app.MapGroup("/api/v{version:apiVersion}/tenants")
			.WithApiVersionSet(versionSet)
			.WithTags("Tenants")
			.RequireAuthorization();

		group.MapGet("/", GetTenants).WithName("GetTenants");
		group.MapGet("/{id:guid}", GetTenantById).WithName("GetTenantById");
		group.MapPost("/", CreateTenant).WithName("CreateTenant");
		group.MapPut("/{id:guid}", UpdateTenant).WithName("UpdateTenant");
		group.MapDelete("/{id:guid}", DeleteTenant).WithName("DeleteTenant");
	}

	private static async Task<IResult> GetTenants(
		int pageNumber,
		int pageSize,
		string? search,
		string? status,
		string? sortBy,
		bool? sortDesc,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var query = new GetTenantsPagedQuery(
			pageNumber > 0 ? pageNumber : 1,
			pageSize > 0 ? pageSize : 10,
			search,
			status,
			sortBy,
			sortDesc ?? false);

		var result = await mediator.Send(query, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<PaginatedList<TenantListResponse>>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> GetTenantById(
		Guid id,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var query = new GetTenantByIdQuery(id);
		var result = await mediator.Send(query, cancellationToken);

		return result.IsFailure
			? Results.NotFound(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<TenantResponse>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> CreateTenant(
		CreateTenantRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new CreateTenantCommand(
			request.Name,
			request.Email,
			request.Phone,
			request.Subdomain,
			request.MaxCompanyCount,
			request.SessionTimeoutMinutes);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<TenantResponse>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> UpdateTenant(
		Guid id,
		UpdateTenantRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new UpdateTenantCommand(
			id,
			request.Name,
			request.Email,
			request.Phone,
			request.Subdomain,
			request.ContactEmail,
			request.ContactPhone,
			request.MaxCompanyCount,
			request.SessionTimeoutMinutes);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<TenantResponse>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> DeleteTenant(
		Guid id,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new DeleteTenantCommand(id);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse.SuccessResponse("Tenant deleted successfully."));
	}
}