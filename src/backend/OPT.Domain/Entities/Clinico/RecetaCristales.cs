using OPT.Domain.Common;

namespace OPT.Domain.Entities.Clinico;

/// <summary>
/// Prescripción óptica del cliente: esfera/cilindro/eje para lejos y cerca, por ojo.
/// OD = ojo derecho, OI = ojo izquierdo.
/// </summary>
public class RecetaCristales : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno (ADR 0004, Ley 21.719).
    /// Dato de salud (sensible bajo Ley 21.719). Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid    PublicId   { get; private set; }

    public int     ClienteId  { get; private set; }
    public Cliente? Cliente   { get; private set; }

    /// <summary>
    /// Orden de Trabajo en la que se materializó esta receta, o null si todavía no se usó en
    /// ninguna (es la receta "pendiente" que deja la ficha clínica). Reproduce el
    /// <c>OPT_RecetaCristales.idOT</c> del legacy: la pestaña Receta de la OT muestra la
    /// prescripción con la que se fabricaron esos cristales, no la última del cliente.
    /// Una OT puede tener más de una receta (2 casos en los datos migrados).
    /// </summary>
    public int?    OrdenDeTrabajoId { get; private set; }

    // Ojo derecho — lejos
    public decimal? OdEsferaLejos   { get; private set; }
    public decimal? OdCilindroLejos { get; private set; }
    public int?     OdEjeLejos      { get; private set; }

    // Ojo derecho — cerca
    public decimal? OdEsferaCerca   { get; private set; }
    public decimal? OdCilindroCerca { get; private set; }
    public int?     OdEjeCerca      { get; private set; }

    // Ojo izquierdo — lejos
    public decimal? OiEsferaLejos   { get; private set; }
    public decimal? OiCilindroLejos { get; private set; }
    public int?     OiEjeLejos      { get; private set; }

    // Ojo izquierdo — cerca
    public decimal? OiEsferaCerca   { get; private set; }
    public decimal? OiCilindroCerca { get; private set; }
    public int?     OiEjeCerca      { get; private set; }

    public bool    Urgente         { get; private set; }
    public bool    RequiereLab     { get; private set; }
    public string? Observaciones   { get; private set; }

    // Distancia pupilar (DP) y adición (ADD) — texto libre, no decimal: el legacy las registra
    // en formatos compuestos/inconsistentes (p. ej. "58-56" para DP OD-OI, "+150/+200" para ADD
    // lejos/cerca) que no caben en una sola columna numérica sin perder información.
    public string? DpLejos    { get; private set; }
    public string? DpCerca    { get; private set; }
    public string? AddLejos   { get; private set; }

    /// <summary>
    /// Replica <c>OPT_RecetaCristales.CheckLejos</c>/<c>CheckCerca</c> del legacy: indican si el
    /// bloque de cristales de Lejos/Cerca aplica a esta receta, independiente de si sus campos
    /// numéricos están completos. En el formulario habilitan los inputs del bloque y exigen sus
    /// tres observaciones (OD/OI/DP); al desmarcar, el bloque completo se limpia.
    /// </summary>
    public bool    IncluirLejos { get; private set; }
    public bool    IncluirCerca { get; private set; }

    // Observación por ojo/DP, separadas para Lejos y Cerca — replica las 6 columnas del legacy
    // (LejosODObservacion, LejosOIObservacion, LejosDPObservacion, CercaODObservacion,
    // CercaOIObservacion, CercaDPObservacion). Coexisten con `Observaciones`, que sigue siendo el
    // campo de notas generales de la receta (y el destino del texto combinado de los datos
    // migrados del legacy — ver OPT.Migracion.RecetaCristalesParser.CombinarObservaciones).
    public string? ObservacionOdLejos { get; private set; }
    public string? ObservacionOiLejos { get; private set; }
    public string? ObservacionDpLejos { get; private set; }
    public string? ObservacionOdCerca { get; private set; }
    public string? ObservacionOiCerca { get; private set; }
    public string? ObservacionDpCerca { get; private set; }

    protected RecetaCristales() { }

    public static RecetaCristales Crear(int clienteId, bool urgente, bool requiereLab,
                                         string? observaciones, int usuarioId)
    {
        var r = new RecetaCristales
        {
            ClienteId    = clienteId,
            Urgente      = urgente,
            RequiereLab  = requiereLab,
            Observaciones = observaciones?.Trim()
        };
        r.SetCreacion(usuarioId);
        return r;
    }

    public void SetOjoDerecho(decimal? esferaLejos, decimal? cilindroLejos, int? ejeLejos,
                               decimal? esferaCerca, decimal? cilindroCerca, int? ejeCerca)
    {
        OdEsferaLejos   = esferaLejos;
        OdCilindroLejos = cilindroLejos;
        OdEjeLejos      = ejeLejos;
        OdEsferaCerca   = esferaCerca;
        OdCilindroCerca = cilindroCerca;
        OdEjeCerca      = ejeCerca;
    }

    public void SetOjoIzquierdo(decimal? esferaLejos, decimal? cilindroLejos, int? ejeLejos,
                                 decimal? esferaCerca, decimal? cilindroCerca, int? ejeCerca)
    {
        OiEsferaLejos   = esferaLejos;
        OiCilindroLejos = cilindroLejos;
        OiEjeLejos      = ejeLejos;
        OiEsferaCerca   = esferaCerca;
        OiCilindroCerca = cilindroCerca;
        OiEjeCerca      = ejeCerca;
    }

    /// <summary>
    /// Asocia la receta a una OT (o la desasocia con <c>null</c>). Es la operación que hace el
    /// mesón al emitir la orden con una receta ya tomada en la ficha clínica.
    /// </summary>
    public void AsociarAOrden(int? ordenDeTrabajoId, int usuarioId)
    {
        OrdenDeTrabajoId = ordenDeTrabajoId;
        SetModificacion(usuarioId);
    }

    public void SetDpAdd(string? dpLejos, string? dpCerca, string? addLejos)
    {
        DpLejos  = dpLejos?.Trim();
        DpCerca  = dpCerca?.Trim();
        AddLejos = addLejos?.Trim();
    }

    /// <summary>Bloque "Incluir Cristales Lejos" — ver comentario de <see cref="IncluirLejos"/>.</summary>
    public void SetInclusionLejos(bool incluirLejos, string? observacionOd, string? observacionOi, string? observacionDp)
    {
        IncluirLejos       = incluirLejos;
        ObservacionOdLejos = observacionOd?.Trim();
        ObservacionOiLejos = observacionOi?.Trim();
        ObservacionDpLejos = observacionDp?.Trim();
    }

    /// <summary>Bloque "Incluir Cristales Cerca" — ver comentario de <see cref="IncluirCerca"/>.</summary>
    public void SetInclusionCerca(bool incluirCerca, string? observacionOd, string? observacionOi, string? observacionDp)
    {
        IncluirCerca       = incluirCerca;
        ObservacionOdCerca = observacionOd?.Trim();
        ObservacionOiCerca = observacionOi?.Trim();
        ObservacionDpCerca = observacionDp?.Trim();
    }

    public void Actualizar(decimal? odEsferaLejos, decimal? odCilindroLejos, int? odEjeLejos,
                            decimal? odEsferaCerca, decimal? odCilindroCerca, int? odEjeCerca,
                            decimal? oiEsferaLejos, decimal? oiCilindroLejos, int? oiEjeLejos,
                            decimal? oiEsferaCerca, decimal? oiCilindroCerca, int? oiEjeCerca,
                            bool urgente, bool requiereLab, string? observaciones,
                            string? dpLejos, string? dpCerca, string? addLejos,
                            bool incluirLejos, string? observacionOdLejos, string? observacionOiLejos, string? observacionDpLejos,
                            bool incluirCerca, string? observacionOdCerca, string? observacionOiCerca, string? observacionDpCerca,
                            int usuarioId)
    {
        SetOjoDerecho(odEsferaLejos, odCilindroLejos, odEjeLejos, odEsferaCerca, odCilindroCerca, odEjeCerca);
        SetOjoIzquierdo(oiEsferaLejos, oiCilindroLejos, oiEjeLejos, oiEsferaCerca, oiCilindroCerca, oiEjeCerca);
        SetDpAdd(dpLejos, dpCerca, addLejos);
        SetInclusionLejos(incluirLejos, observacionOdLejos, observacionOiLejos, observacionDpLejos);
        SetInclusionCerca(incluirCerca, observacionOdCerca, observacionOiCerca, observacionDpCerca);
        Urgente       = urgente;
        RequiereLab   = requiereLab;
        Observaciones = observaciones?.Trim();
        SetModificacion(usuarioId);
    }
}
