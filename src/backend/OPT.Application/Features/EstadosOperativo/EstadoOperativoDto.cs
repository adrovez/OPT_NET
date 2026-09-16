namespace OPT.Application.Features.EstadosOperativo;

/// <summary>
/// Estado del ciclo de vida de un Operativo. <c>EsTerminal</c> y <c>PuedeAnularse</c> viajan al
/// frontend para que sepa cuándo deshabilitar acciones sin duplicar la regla del dominio.
/// </summary>
public record EstadoOperativoDto(int Id, string Nombre, bool EsTerminal, bool PuedeAnularse);
