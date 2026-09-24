using MediatR;

namespace OPT.Application.Features.Productos.Commands.Actualizar;

public record ActualizarProductoCommand(
    int Id, string Codigo, string Descripcion, bool ControlStock, int? CategoriaId = null) : IRequest<ProductoDto>;
