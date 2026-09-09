using MediatR;
using OPT.Domain.Common;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Clientes.Queries.ObtenerTodos;

public sealed class ObtenerClientesQueryHandler(IClienteRepositorio clienteRepo)
    : IRequestHandler<ObtenerClientesQuery, PagedResult<ClienteDto>>
{
    public async Task<PagedResult<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken ct)
    {
        var (items, total) = await clienteRepo.BuscarPaginadoAsync(request, ct);

        return PagedResultFactory.Crear(items, total, request, c => new ClienteDto(
            c.PublicId, c.Rut, c.Nombre, c.Apellido, c.Email, c.Telefono, c.Direccion,
            c.ComunaId, c.FechaNacimiento, c.TipoPrevision));
    }
}
