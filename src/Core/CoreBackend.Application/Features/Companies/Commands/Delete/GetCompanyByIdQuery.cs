using MediatR;
using CoreBackend.Contracts.Companies.Responses;
using CoreBackend.Domain.Common.Primitives;

namespace CoreBackend.Application.Features.Companies.Queries.GetById;

public record GetCompanyByIdQuery(Guid Id) : IRequest<Result<CompanyResponse>>;