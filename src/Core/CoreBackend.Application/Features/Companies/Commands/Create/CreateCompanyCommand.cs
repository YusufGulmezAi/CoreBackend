using MediatR;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Companies.Commands.Create;

public record CreateCompanyCommand(
	string Name,
	string Code,
	string? TaxNumber,
	string? Address,
	string? Phone,
	string? Email
) : IRequest<Result<CompanyResponse>>;