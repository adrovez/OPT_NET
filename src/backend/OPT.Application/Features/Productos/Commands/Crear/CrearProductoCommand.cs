using MediatR;

namespace OPT.Application.Features.Productos.Commands.Crear;

/// <summary>
/// CategoriaId es opcional (por defecto "General"). Descripcion solo se usa en categorías que no
/// exigen Marca/Modelo; en Marcos y Lentes de Sol se calcula y lo enviado se ignora (ADR 0013).
/// </summary>
public record CrearProductoCommand(
    string Codigo, string? Descripcion, bool ControlStock, int? CategoriaId = null,
    string? Marca = null, string? Modelo = null, string? Color = null) : IRequest<ProductoDto>;
