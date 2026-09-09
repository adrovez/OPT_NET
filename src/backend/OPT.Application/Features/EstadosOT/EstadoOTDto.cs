namespace OPT.Application.Features.EstadosOT;

/// <summary>
/// Estado del ciclo de vida de una OT. <c>EsTerminal</c> viaja al frontend para que sepa
/// cuándo deshabilitar las acciones de avance/retroceso sin duplicar la regla del dominio.
/// </summary>
public record EstadoOTDto(int Id, string Nombre, bool EsTerminal);
