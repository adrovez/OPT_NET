namespace OPT.Domain.Entities.Operativo;

/// <summary>
/// Ids fijos del catálogo <c>OPT_EstadoOperativo</c> (script <c>009_modulo_operativo.sql</c>) y
/// reglas de transición del ciclo de vida de un Operativo.
///
/// Flujo (requerimiento <c>OPT_Requerimiento_Modulo_Operativo.md</c>, sección 5):
/// <c>Prospecto → Ingresado → Cobranza → Cerrado</c>, con <see cref="Anulado"/> como salida
/// alternativa. A diferencia de <c>EstadosOT</c>, el requerimiento no pide retroceso de etapa
/// para el Operativo — solo avance secuencial de a un paso.
///
/// Decisión 2026-09-15 (punto abierto 8.6 del requerimiento, resuelta con el usuario): la
/// anulación solo es posible desde <see cref="Prospecto"/> o <see cref="Ingresado"/> — una vez
/// en <see cref="Cobranza"/> ya no se puede anular, solo <see cref="Cerrado"/>.
/// </summary>
public static class EstadosOperativo
{
    public const int Prospecto = 1;
    public const int Ingresado = 2;
    public const int Cobranza  = 3;
    public const int Cerrado   = 4;
    public const int Anulado   = 5;

    /// <summary>Estado con el que nace todo Operativo.</summary>
    public const int Inicial = Prospecto;

    /// <summary>Último estado de la secuencia operativa (sin contar <see cref="Anulado"/>).</summary>
    public const int UltimoDelFlujo = Cerrado;

    /// <summary>Un estado terminal no admite más transiciones.</summary>
    public static bool EsTerminal(int estadoId) => estadoId is Cerrado or Anulado;

    /// <summary>Verdadero si el id pertenece a la secuencia operativa (no incluye <see cref="Anulado"/>).</summary>
    public static bool PerteneceAlFlujo(int estadoId) => estadoId is >= Prospecto and <= Cerrado;

    /// <summary>Solo se puede anular desde el inicio del flujo, antes de entrar a cobranza.</summary>
    public static bool PuedeAnularseDesde(int estadoId) => estadoId is Prospecto or Ingresado;
}
