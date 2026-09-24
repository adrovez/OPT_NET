using MediatR;

namespace OPT.Application.Features.Productos.Commands.DarDeBaja;

/// <summary>Baja lógica: el producto sale del catálogo pero las OT que ya lo usan conservan su detalle.</summary>
public record DarDeBajaProductoCommand(int Id) : IRequest;
