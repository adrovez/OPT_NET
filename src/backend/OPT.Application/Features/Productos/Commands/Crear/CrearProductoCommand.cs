using MediatR;

namespace OPT.Application.Features.Productos.Commands.Crear;

/// <summary>CategoriaId es opcional: hoy el catálogo solo tiene "General" (Id 1), que se usa por defecto.</summary>
public record CrearProductoCommand(
    string Codigo, string Descripcion, bool ControlStock, int? CategoriaId = null) : IRequest<ProductoDto>;
