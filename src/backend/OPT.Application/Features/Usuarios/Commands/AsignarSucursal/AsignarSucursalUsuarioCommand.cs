using MediatR;

namespace OPT.Application.Features.Usuarios.Commands.AsignarSucursal;

public record AsignarSucursalUsuarioCommand(Guid PublicId, int SucursalId) : IRequest;
