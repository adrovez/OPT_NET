using MediatR;
using OPT.Application.Features.Empresas;

namespace OPT.Application.Features.Empresas.Queries.ObtenerPorId;

public record ObtenerEmpresaPorPublicIdQuery(Guid PublicId) : IRequest<EmpresaDto>;
