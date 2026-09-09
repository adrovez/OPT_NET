using MediatR;

namespace OPT.Application.Features.Usuarios.Commands.QuitarSucursal;

public record QuitarSucursalUsuarioCommand(Guid PublicId, int SucursalId) : IRequest;
