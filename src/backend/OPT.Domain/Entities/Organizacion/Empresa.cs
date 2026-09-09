using OPT.Domain.Common;

namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Empresa dueña de la(s) óptica(s).
/// Mejoras vs. legacy: RUT como atributo único (no PK), RazonSocial separado de nombre,
/// Giro y Email incorporados, Contacto (persona de contacto, del legacy OPT_Empresa.Contacto).
/// </summary>
public sealed class Empresa : AuditableEntity
{
    /// <summary>
    /// Identificador no enumerable expuesto en API/URLs — nunca el Id interno (ADR 0004, Ley 21.719).
    /// Generado por la base de datos (DEFAULT NEWID()).
    /// </summary>
    public Guid    PublicId    { get; private set; }

    public string  Nombre      { get; private set; } = string.Empty;
    public string  Rut         { get; private set; } = string.Empty;
    public string  RazonSocial { get; private set; } = string.Empty;
    public string  Giro        { get; private set; } = string.Empty;
    public string  Direccion   { get; private set; } = string.Empty;
    public string  Telefono    { get; private set; } = string.Empty;
    public string  Email       { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre de la persona de contacto en la empresa.
    /// Corresponde a OPT_Empresa.Contacto varchar(50) en el legacy.
    /// </summary>
    public string  Contacto    { get; private set; } = string.Empty;

    private readonly List<EmpresaSucursal> _sucursales = [];
    public IReadOnlyCollection<EmpresaSucursal> Sucursales => _sucursales.AsReadOnly();

    // Constructor para EF Core
    private Empresa() { }

    public static Empresa Crear(
        string nombre, string rut, string razonSocial, string giro,
        string direccion, string telefono, string email, string contacto,
        int creadoPor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(rut);

        var empresa = new Empresa
        {
            Nombre      = nombre.Trim(),
            Rut         = rut.Trim(),
            RazonSocial = razonSocial.Trim(),
            Giro        = giro.Trim(),
            Direccion   = direccion.Trim(),
            Telefono    = telefono.Trim(),
            Email       = email.Trim(),
            Contacto    = contacto.Trim()
        };
        empresa.SetCreacion(creadoPor);
        return empresa;
    }

    public void Actualizar(
        string nombre, string rut, string razonSocial, string giro,
        string direccion, string telefono, string email, string contacto,
        int modificadoPor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(rut);

        Nombre      = nombre.Trim();
        Rut         = rut.Trim();
        RazonSocial = razonSocial.Trim();
        Giro        = giro.Trim();
        Direccion   = direccion.Trim();
        Telefono    = telefono.Trim();
        Email       = email.Trim();
        Contacto    = contacto.Trim();
        SetModificacion(modificadoPor);
    }
}
