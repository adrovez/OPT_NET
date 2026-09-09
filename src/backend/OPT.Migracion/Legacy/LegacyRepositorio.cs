using Dapper;
using Microsoft.Data.SqlClient;

namespace OPT.Migracion.Legacy;

/// <summary>Lecturas de solo consulta contra db_a25cfd_opt2. Nunca escribe en el legacy.</summary>
public sealed class LegacyRepositorio(string connectionString)
{
    private SqlConnection Conectar() => new(connectionString);

    public async Task<IReadOnlyList<LegacyRegion>> ObtenerRegionesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyRegion>(
            "SELECT idRegion AS IdRegion, Region FROM dbo.OPT_Region ORDER BY idRegion;");
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyComuna>> ObtenerComunasAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyComuna>(
            "SELECT idComuna AS IdComuna, idRegion AS IdRegion, Comuna FROM dbo.OPT_Comuna ORDER BY idComuna;");
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyRol>> ObtenerRolesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyRol>(
            "SELECT idRol AS IdRol, Rol FROM dbo.OPT_Rol ORDER BY idRol;");
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacySucursal>> ObtenerSucursalesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacySucursal>(
            """
            SELECT idSucursal AS IdSucursal, Nombre, Direccion, Telefono, FechaRegistro, Matriz
            FROM dbo.OPT_Sucursal
            ORDER BY idSucursal;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyEmpresa>> ObtenerEmpresasAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyEmpresa>(
            """
            SELECT idEmpresa AS IdEmpresa, Empresa, Direccion, Contacto, Telefono
            FROM dbo.OPT_Empresa
            ORDER BY idEmpresa;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyEmpresaSucursal>> ObtenerEmpresaSucursalesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyEmpresaSucursal>(
            """
            SELECT idEmpresaSucursal AS IdEmpresaSucursal, idEmpresa AS IdEmpresa, idSucursal AS IdSucursal
            FROM dbo.OPT_EmpresaSucursal
            ORDER BY idEmpresaSucursal;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyUsuario>> ObtenerUsuariosAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyUsuario>(
            """
            SELECT RutUsuario, Nombre, Mail, Clave, FechaIngreso, idRol AS IdRol
            FROM dbo.OPT_Usuario
            ORDER BY RutUsuario;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyUsuarioSucursal>> ObtenerUsuarioSucursalesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyUsuarioSucursal>(
            """
            SELECT idUsuarioSucursal AS IdUsuarioSucursal, RutUsuario, idSucursal AS IdSucursal
            FROM dbo.OPT_UsuarioSucursal
            ORDER BY idUsuarioSucursal;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyCliente>> ObtenerClientesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyCliente>(
            """
            SELECT RutCliente, Nombre, Direccion, idComuna AS IdComuna, Celular, Mail,
                   FechaIngreso, FechaNacimiento, TipoPrevision
            FROM dbo.OPT_Cliente
            ORDER BY RutCliente;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyAnamnesis>> ObtenerAnamnesisAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyAnamnesis>(
            """
            SELECT idAnamnesis AS IdAnamnesis, RutCliente, FechaIngreso, RutUsuario,
                   hipertension AS Hipertension, Diabetes, Alergias, Lentes, Observacion
            FROM dbo.OPT_Anamnesis
            ORDER BY idAnamnesis;
            """);
        return filas.ToList();
    }

    public async Task<IReadOnlyList<LegacyRecetaCristales>> ObtenerRecetasCristalesAsync()
    {
        using var con = Conectar();
        var filas = await con.QueryAsync<LegacyRecetaCristales>(
            """
            SELECT idRecetaCristales AS IdRecetaCristales, RutCliente, FechaIngreso,
                   LejosODEsferico, LejosODCilindro, LejosODEje, LejosODObservacion,
                   LejosOIEsferico, LejosOICilindro, LejosOIEje, LejosOIObservacion,
                   CercaODEsferico, CercaODCilindro, CercaODEje, CercaODObservacion,
                   CercaOIEsferico, CercaOICilindro, CercaOIEje, CercaOIObservacion,
                   LejosDPEsferico, LejosDPObservacion,
                   CercaDPEsferico, CercaDPObservacion,
                   LejosADDEsfera,
                   CheckCristalesLaboratorio, CheckUrgente
            FROM dbo.OPT_RecetaCristales
            ORDER BY idRecetaCristales;
            """);
        return filas.ToList();
    }
}
