namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Ids fijos del catálogo <c>OPT_EstadoOT</c> y reglas de transición del ciclo de vida
/// de una Orden de Trabajo.
///
/// Los ids 0..6 son los heredados del legacy; <see cref="Anulado"/> (7) lo agrega el
/// sistema nuevo (script <c>005_ot_publicid_estado_anulado.sql</c>) para modelar como
/// estado lo que el legacy resolvía con <c>SP_OTEliminar</c>.
///
/// Flujo decidido con el negocio (2026-08-27): <b>secuencial con avance/retroceso
/// controlado</b> — se avanza al estado siguiente o se retrocede uno solo (con
/// observación obligatoria); no se saltan etapas. <see cref="Entregado"/> y
/// <see cref="Anulado"/> son terminales.
/// </summary>
public static class EstadosOT
{
    public const int Ingresado   = 0;
    public const int EnProceso   = 1;
    public const int Montaje     = 2;
    public const int Laboratorio = 3;
    public const int Calidad     = 4;
    public const int Despacho    = 5;
    public const int Entregado   = 6;
    public const int Anulado     = 7;

    /// <summary>Estado con el que nace toda OT.</summary>
    public const int Inicial = Ingresado;

    /// <summary>Último estado de la secuencia operativa (sin contar <see cref="Anulado"/>).</summary>
    public const int UltimoDelFlujo = Entregado;

    /// <summary>Un estado terminal no admite más transiciones.</summary>
    public static bool EsTerminal(int estadoId) => estadoId is Entregado or Anulado;

    /// <summary>Verdadero si el id pertenece a la secuencia operativa (no incluye <see cref="Anulado"/>).</summary>
    public static bool PerteneceAlFlujo(int estadoId) => estadoId is >= Ingresado and <= Entregado;
}
