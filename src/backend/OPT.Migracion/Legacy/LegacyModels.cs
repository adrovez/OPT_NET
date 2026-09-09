namespace OPT.Migracion.Legacy;

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Region.</summary>
public sealed record LegacyRegion(int IdRegion, string Region);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Comuna.</summary>
public sealed record LegacyComuna(int IdComuna, int IdRegion, string Comuna);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Rol.</summary>
public sealed record LegacyRol(int IdRol, string Rol);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Sucursal.</summary>
public sealed record LegacySucursal(int IdSucursal, string Nombre, string Direccion, string Telefono, DateTime FechaRegistro, bool Matriz);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Empresa.</summary>
public sealed record LegacyEmpresa(int IdEmpresa, string Empresa, string? Direccion, string? Contacto, string? Telefono);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_EmpresaSucursal.</summary>
public sealed record LegacyEmpresaSucursal(int IdEmpresaSucursal, int IdEmpresa, int IdSucursal);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Usuario (incluye Clave en texto plano — solo en memoria, nunca se persiste así).</summary>
public sealed record LegacyUsuario(string RutUsuario, string Nombre, string Mail, string Clave, DateTime FechaIngreso, int? IdRol);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_UsuarioSucursal.</summary>
public sealed record LegacyUsuarioSucursal(int IdUsuarioSucursal, string RutUsuario, int IdSucursal);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Cliente.</summary>
public sealed record LegacyCliente(
    string RutCliente, string? Nombre, string? Direccion, int? IdComuna,
    string? Celular, string? Mail, DateTime? FechaIngreso, DateTime? FechaNacimiento,
    string? TipoPrevision);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_Anamnesis.</summary>
public sealed record LegacyAnamnesis(
    long IdAnamnesis, string RutCliente, DateTime FechaIngreso, string RutUsuario,
    bool Hipertension, bool Diabetes, bool Alergias, bool Lentes, string? Observacion);

/// <summary>Fila cruda de db_a25cfd_opt2.dbo.OPT_RecetaCristales.</summary>
public sealed record LegacyRecetaCristales(
    long IdRecetaCristales, string? RutCliente, DateTime? FechaIngreso,
    string? LejosODEsferico, string? LejosODCilindro, string? LejosODEje, string? LejosODObservacion,
    string? LejosOIEsferico, string? LejosOICilindro, string? LejosOIEje, string? LejosOIObservacion,
    string? CercaODEsferico, string? CercaODCilindro, string? CercaODEje, string? CercaODObservacion,
    string? CercaOIEsferico, string? CercaOICilindro, string? CercaOIEje, string? CercaOIObservacion,
    string? LejosDPEsferico, string? LejosDPObservacion,
    string? CercaDPEsferico, string? CercaDPObservacion,
    string? LejosADDEsfera,
    bool CheckCristalesLaboratorio, bool CheckUrgente);
