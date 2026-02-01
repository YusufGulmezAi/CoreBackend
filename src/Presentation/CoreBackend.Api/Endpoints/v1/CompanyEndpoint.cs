using Asp.Versioning;
using MediatR;
using CoreBackend.Application.Features.Companies.Commands.Create;
using CoreBackend.Application.Features.Companies.Commands.Update;
using CoreBackend.Application.Features.Companies.Commands.Delete;
using CoreBackend.Application.Features.Companies.Queries.GetById;
using CoreBackend.Application.Features.Companies.Queries.GetPaged;
using CoreBackend.Contracts.Common;
using CoreBackend.Contracts.Companies.Requests;

namespace CoreBackend.Api.Endpoints.v1;

public class CompanyEndpoint : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		var versionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1, 0))
			.Build();

		var group = app.MapGroup("/api/v{version:apiVersion}/companies")
			.WithApiVersionSet(versionSet)
			.WithTags("Companies")
			.RequireAuthorization();

		group.MapGet("", GetPaged).WithName("GetCompanies");
		group.MapGet("/{id:guid}", GetById).WithName("GetCompanyById");
		group.MapPost("", Create).WithName("CreateCompany");
		group.MapPut("/{id:guid}", Update).WithName("UpdateCompany");
		group.MapDelete("/{id:guid}", Delete).WithName("DeleteCompany");
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
		var query = new GetCompaniesPagedQuery(pageNumber, pageSize, search, sortBy, sortDesc);
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
		var query = new GetCompanyByIdQuery(id);
		var result = await mediator.Send(query, cancellationToken);

		return result.IsFailure
			? Results.NotFound(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Ok(ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Create(
		CreateCompanyRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new CreateCompanyCommand(
			request.Name,
			request.Code,
			request.TaxNumber,
			request.Address,
			request.Phone,
			request.Email);

		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.Created($"/api/v1/companies/{result.Value.Id}", ApiResponse<object>.SuccessResponse(result.Value));
	}

	private static async Task<IResult> Update(
		Guid id,
		UpdateCompanyRequest request,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new UpdateCompanyCommand(
			id,
			request.Name,
			request.TaxNumber,
			request.Address,
			request.Phone,
			request.Email);

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
		var command = new DeleteCompanyCommand(id);
		var result = await mediator.Send(command, cancellationToken);

		return result.IsFailure
			? Results.BadRequest(ApiResponse<object>.FailureResponse(result.Error.Message, result.Error.Code))
			: Results.NoContent();
	}
}