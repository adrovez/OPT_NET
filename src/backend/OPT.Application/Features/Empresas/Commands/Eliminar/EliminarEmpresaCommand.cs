using MediatR;

namespace OPT.Application.Features.Empresas.Commands.Eliminar;

public record EliminarEmpresaCommand(Guid PublicId) : IRequest;
