using MediatR;
using OPT.Application.Features.Sucursales;

namespace OPT.Application.Features.Sucursales.Queries.ObtenerPorId;

public record ObtenerSucursalPorIdQuery(int Id) : IRequest<SucursalDto>;
