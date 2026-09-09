namespace OPT.Migracion.Destino;

/// <summary>Fila cruda de dbOPT_NET.dbo.OPT_Region.</summary>
public sealed record DestinoRegion(int Id, string Nombre);

/// <summary>Fila cruda de dbOPT_NET.dbo.OPT_Comuna.</summary>
public sealed record DestinoComuna(int Id, string Nombre, int RegionId);

/// <summary>Fila cruda de dbOPT_NET.dbo.OPT_Rol.</summary>
public sealed record DestinoRol(int Id, string Nombre);

/// <summary>Datos ya normalizados listos para insertar en OPT_Usuario.</summary>
public sealed record NuevoUsuario(
    string RutLegacy, string Rut, string Nombre, string Apellido, string Email,
    string ClaveHash, int RolId, DateTimeOffset CreadoEn);

/// <summary>Datos ya normalizados listos para insertar en OPT_Sucursal.</summary>
public sealed record NuevaSucursal(
    int IdSucursalLegacy, string Nombre, string Direccion, string Telefono,
    bool EsMatriz, DateTimeOffset CreadoEn);

/// <summary>Datos ya normalizados listos para insertar en OPT_Empresa.</summary>
public sealed record NuevaEmpresa(
    int IdEmpresaLegacy, string Nombre, string Rut, string RazonSocial,
    string Direccion, string Telefono, string Contacto, DateTimeOffset CreadoEn);

/// <summary>Datos ya normalizados listos para insertar en OPT_Cliente.</summary>
public sealed record NuevoCliente(
    string RutLegacy, string Rut, string Nombre, string Apellido, string? Email,
    string? Telefono, string? Direccion, int? ComunaId, DateOnly? FechaNacimiento,
    string? TipoPrevision, DateTimeOffset CreadoEn);

/// <summary>Datos ya normalizados listos para insertar en OPT_Anamnesis.</summary>
public sealed record NuevaAnamnesis(
    int ClienteId, bool Hipertension, bool Diabetes, bool Alergias,
    bool UsaLentesPrevio, string? Observaciones, DateTimeOffset CreadoEn);

/// <summary>Datos ya normalizados listos para insertar en OPT_RecetaCristales.</summary>
public sealed record NuevaRecetaCristales(
    int ClienteId,
    decimal? OdEsferaLejos, decimal? OdCilindroLejos, int? OdEjeLejos,
    decimal? OdEsferaCerca, decimal? OdCilindroCerca, int? OdEjeCerca,
    decimal? OiEsferaLejos, decimal? OiCilindroLejos, int? OiEjeLejos,
    decimal? OiEsferaCerca, decimal? OiCilindroCerca, int? OiEjeCerca,
    bool Urgente, bool RequiereLab, string? Observaciones,
    string? DpLejos, string? DpCerca, string? AddLejos,
    DateTimeOffset CreadoEn);
