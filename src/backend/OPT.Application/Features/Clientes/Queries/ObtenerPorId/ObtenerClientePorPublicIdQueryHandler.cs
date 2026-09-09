using MediatR;
using OPT.Application.Common.Exceptions;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Clientes.Queries.ObtenerPorId;

public sealed class ObtenerClientePorPublicIdQueryHandler(IClienteRepositorio clienteRepo)
    : IRequestHandler<ObtenerClientePorPublicIdQuery, ClienteDto>
{
    public async Task<ClienteDto> Handle(ObtenerClientePorPublicIdQuery request, CancellationToken ct)
    {
        var cliente = await clienteRepo.ObtenerPorPublicIdAsync(request.PublicId, ct)
            ?? throw new NotFoundException("Cliente", request.PublicId);

        return new ClienteDto(cliente.PublicId, cliente.Rut, cliente.Nombre, cliente.Apellido,
            cliente.Email, cliente.Telefono, cliente.Direccion, cliente.ComunaId,
            cliente.FechaNacimiento, cliente.TipoPrevision);
    }
}
