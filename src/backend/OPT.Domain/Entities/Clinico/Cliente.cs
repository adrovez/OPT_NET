using OPT.Domain.Common;

namespace OPT.Domain.Entities.Clinico;

/// <summary>
/// Persona atendida por la óptica (paciente/beneficiario).
/// El RUT es atributo único, NO la clave primaria (ADR 0003 — corrección del legacy).
/// </summary>
public class Cliente : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno (ADR 0004, Ley 21.719).
    /// Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid    PublicId   { get; private set; }

    public string  Rut        { get; private set; } = string.Empty;
    public string  Nombre     { get; private set; } = string.Empty;
    public string  Apellido   { get; private set; } = string.Empty;
    public string? Email      { get; private set; }
    public string? Telefono   { get; private set; }
    public string? Direccion  { get; private set; }
    public int?    ComunaId   { get; private set; }
    public DateOnly? FechaNacimiento { get; private set; }
    public string?   TipoPrevision   { get; private set; }

    private readonly List<Anamnesis>       _anamnesis        = [];
    private readonly List<RecetaCristales> _recetas          = [];

    public IReadOnlyCollection<Anamnesis>       Anamnesis => _anamnesis.AsReadOnly();
    public IReadOnlyCollection<RecetaCristales> Recetas   => _recetas.AsReadOnly();

    protected Cliente() { }

    public static Cliente Crear(string rut, string nombre, string apellido,
                                 int usuarioId, string? email = null,
                                 string? telefono = null, string? direccion = null,
                                 int? comunaId = null, DateOnly? fechaNacimiento = null,
                                 string? tipoPrevision = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rut);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        var c = new Cliente
        {
            Rut       = rut.Trim().ToUpperInvariant(),
            Nombre    = nombre.Trim(),
            Apellido  = apellido.Trim(),
            Email     = email?.Trim().ToLowerInvariant(),
            Telefono  = telefono?.Trim(),
            Direccion = direccion?.Trim(),
            ComunaId  = comunaId,
            FechaNacimiento = fechaNacimiento,
            TipoPrevision   = tipoPrevision?.Trim()
        };
        c.SetCreacion(usuarioId);
        return c;
    }

    public void Actualizar(string nombre, string apellido, string? email,
                            string? telefono, string? direccion, int? comunaId, int usuarioId,
                            DateOnly? fechaNacimiento = null, string? tipoPrevision = null)
    {
        Nombre    = nombre.Trim();
        Apellido  = apellido.Trim();
        Email     = email?.Trim().ToLowerInvariant();
        Telefono  = telefono?.Trim();
        Direccion = direccion?.Trim();
        ComunaId  = comunaId;
        FechaNacimiento = fechaNacimiento;
        TipoPrevision   = tipoPrevision?.Trim();
        SetModificacion(usuarioId);
    }
}
