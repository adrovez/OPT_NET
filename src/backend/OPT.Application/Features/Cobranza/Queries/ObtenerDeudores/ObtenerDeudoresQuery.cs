using MediatR;

namespace OPT.Application.Features.Cobranza.Queries.ObtenerDeudores;

/// <summary>
/// Deuda vigente consolidada por empresa convenio, ordenada de mayor a menor saldo.
/// No es paginada: son tantas filas como empresas con deuda (cientos, no miles) y la
/// pantalla necesita el total general.
/// </summary>
public record ObtenerDeudoresQuery : IRequest<IReadOnlyList<DeudorEmpresaDto>>;
