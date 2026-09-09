using OPT.Domain.Common;

namespace OPT.Domain.Entities.Clinico;

/// <summary>
/// Ficha de antecedentes de salud del cliente relevantes para la atención óptica.
/// Regla de negocio: es inmutable una vez creada — no existe método de actualización
/// (a diferencia de otras entidades clínicas). Solo admite <see cref="Crear"/> y baja lógica.
/// </summary>
public class Anamnesis : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno (ADR 0004, Ley 21.719).
    /// Dato de salud (sensible bajo Ley 21.719). Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid    PublicId      { get; private set; }

    public int     ClienteId     { get; private set; }
    public Cliente? Cliente      { get; private set; }

    public bool    Hipertension  { get; private set; }
    public bool    Diabetes      { get; private set; }
    public bool    Alergias      { get; private set; }
    public string? DetalleAlergias { get; private set; }
    public bool    UsaLentesPrevio { get; private set; }
    public string? Observaciones  { get; private set; }

    protected Anamnesis() { }

    public static Anamnesis Crear(int clienteId, bool hipertension, bool diabetes,
                                   bool alergias, string? detalleAlergias,
                                   bool usaLentesPrevio, string? observaciones, int usuarioId)
    {
        var a = new Anamnesis
        {
            ClienteId       = clienteId,
            Hipertension    = hipertension,
            Diabetes        = diabetes,
            Alergias        = alergias,
            DetalleAlergias = detalleAlergias?.Trim(),
            UsaLentesPrevio = usaLentesPrevio,
            Observaciones   = observaciones?.Trim()
        };
        a.SetCreacion(usuarioId);
        return a;
    }
}
