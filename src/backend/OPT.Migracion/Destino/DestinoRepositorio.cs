using Dapper;
using Microsoft.Data.SqlClient;

namespace OPT.Migracion.Destino;

/// <summary>Lecturas y escrituras contra dbOPT_NET. Las escrituras siempre corren dentro de una transacción.</summary>
public sealed class DestinoRepositorio(string connectionString)
{
    public SqlConnection Conectar() => new(connectionString);

    public async Task<IReadOnlyList<DestinoRegion>> ObtenerRegionesAsync(SqlConnection con)
    {
        var filas = await con.QueryAsync<DestinoRegion>(
            "SELECT Id, Nombre FROM dbo.OPT_Region ORDER BY Id;");
        return filas.ToList();
    }

    public async Task<IReadOnlyList<DestinoComuna>> ObtenerComunasAsync(SqlConnection con)
    {
        var filas = await con.QueryAsync<DestinoComuna>(
            "SELECT Id, Nombre, RegionId FROM dbo.OPT_Comuna ORDER BY Id;");
        return filas.ToList();
    }

    public async Task<IReadOnlyList<DestinoRol>> ObtenerRolesAsync(SqlConnection con)
    {
        var filas = await con.QueryAsync<DestinoRol>(
            "SELECT Id, Nombre FROM dbo.OPT_Rol ORDER BY Id;");
        return filas.ToList();
    }

    /// <summary>Cuenta filas actuales en destino — guardia de re-ejecución antes de escribir.</summary>
    public async Task<int> ContarFilasAsync(SqlConnection con, string tabla)
    {
        return await con.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM dbo.{tabla};");
    }

    /// <summary>Inserta un Usuario y retorna su nuevo UsuarioId (identity). CreadoPor debe ser un UsuarioId válido o el propio Id (autoreferencia, ver Program.cs).</summary>
    public async Task<int> InsertarUsuarioAsync(SqlConnection con, SqlTransaction tx, NuevoUsuario u, int creadoPor)
    {
        return await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.OPT_Usuario
                (Rut, Nombre, Apellido, Email, ClaveHash, RolId, SucursalActivaId, Activo, CreadoEn, CreadoPor)
            OUTPUT INSERTED.UsuarioId
            VALUES
                (@Rut, @Nombre, @Apellido, @Email, @ClaveHash, @RolId, NULL, 1, @CreadoEn, @CreadoPor);
            """,
            new { u.Rut, u.Nombre, u.Apellido, u.Email, u.ClaveHash, u.RolId, u.CreadoEn, CreadoPor = creadoPor },
            tx);
    }

    /// <summary>Autoreferencia CreadoPor del usuario "bootstrap" (el primer admin migrado) hacia su propio Id.</summary>
    public async Task ActualizarCreadoPorPropioAsync(SqlConnection con, SqlTransaction tx, int usuarioId)
    {
        await con.ExecuteAsync(
            "UPDATE dbo.OPT_Usuario SET CreadoPor = @UsuarioId WHERE UsuarioId = @UsuarioId;",
            new { UsuarioId = usuarioId }, tx);
    }

    public async Task<int> InsertarSucursalAsync(SqlConnection con, SqlTransaction tx, NuevaSucursal s, int creadoPor)
    {
        return await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.OPT_Sucursal (Nombre, Direccion, Telefono, EsMatriz, CreadoEn, CreadoPor)
            OUTPUT INSERTED.SucursalId
            VALUES (@Nombre, @Direccion, @Telefono, @EsMatriz, @CreadoEn, @CreadoPor);
            """,
            new { s.Nombre, s.Direccion, s.Telefono, s.EsMatriz, s.CreadoEn, CreadoPor = creadoPor },
            tx);
    }

    public async Task<int> InsertarEmpresaAsync(SqlConnection con, SqlTransaction tx, NuevaEmpresa e, int creadoPor)
    {
        return await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.OPT_Empresa
                (Nombre, Rut, RazonSocial, Giro, Direccion, Telefono, Email, Contacto, CreadoEn, CreadoPor)
            OUTPUT INSERTED.EmpresaId
            VALUES
                (@Nombre, @Rut, @RazonSocial, '', @Direccion, @Telefono, '', @Contacto, @CreadoEn, @CreadoPor);
            """,
            new { e.Nombre, e.Rut, e.RazonSocial, e.Direccion, e.Telefono, e.Contacto, e.CreadoEn, CreadoPor = creadoPor },
            tx);
    }

    public async Task InsertarUsuarioSucursalAsync(SqlConnection con, SqlTransaction tx, int usuarioId, int sucursalId)
    {
        await con.ExecuteAsync(
            "INSERT INTO dbo.OPT_UsuarioSucursal (UsuarioId, SucursalId) VALUES (@UsuarioId, @SucursalId);",
            new { UsuarioId = usuarioId, SucursalId = sucursalId }, tx);
    }

    public async Task InsertarEmpresaSucursalAsync(SqlConnection con, SqlTransaction tx, int empresaId, int sucursalId)
    {
        await con.ExecuteAsync(
            "INSERT INTO dbo.OPT_EmpresaSucursal (EmpresaId, SucursalId) VALUES (@EmpresaId, @SucursalId);",
            new { EmpresaId = empresaId, SucursalId = sucursalId }, tx);
    }

    /// <summary>Busca el UsuarioId ya migrado que coincide con un RUT (normalizado igual que Usuario.Crear()).</summary>
    public async Task<int?> ObtenerUsuarioIdPorRutAsync(SqlConnection con, SqlTransaction tx, string rut)
    {
        return await con.ExecuteScalarAsync<int?>(
            "SELECT UsuarioId FROM dbo.OPT_Usuario WHERE Rut = @Rut;",
            new { Rut = rut }, tx);
    }

    public async Task<int> InsertarClienteAsync(SqlConnection con, SqlTransaction tx, NuevoCliente c, int creadoPor)
    {
        return await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.OPT_Cliente
                (Rut, Nombre, Apellido, Email, Telefono, Direccion, ComunaId, FechaNacimiento, TipoPrevision, CreadoEn, CreadoPor)
            OUTPUT INSERTED.ClienteId
            VALUES
                (@Rut, @Nombre, @Apellido, @Email, @Telefono, @Direccion, @ComunaId, @FechaNacimiento, @TipoPrevision, @CreadoEn, @CreadoPor);
            """,
            new
            {
                c.Rut, c.Nombre, c.Apellido, c.Email, c.Telefono, c.Direccion, c.ComunaId,
                // Dapper/SqlClient no soportan System.DateOnly como parámetro directo — se convierte a DateTime.
                FechaNacimiento = c.FechaNacimiento.HasValue ? c.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                c.TipoPrevision, c.CreadoEn, CreadoPor = creadoPor
            },
            tx);
    }

    public async Task<int> InsertarAnamnesisAsync(SqlConnection con, SqlTransaction tx, NuevaAnamnesis a, int creadoPor)
    {
        return await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.OPT_Anamnesis
                (ClienteId, Hipertension, Diabetes, Alergias, DetalleAlergias, UsaLentesPrevio, Observaciones, CreadoEn, CreadoPor)
            OUTPUT INSERTED.AnamnesisId
            VALUES
                (@ClienteId, @Hipertension, @Diabetes, @Alergias, NULL, @UsaLentesPrevio, @Observaciones, @CreadoEn, @CreadoPor);
            """,
            new { a.ClienteId, a.Hipertension, a.Diabetes, a.Alergias, a.UsaLentesPrevio, a.Observaciones, a.CreadoEn, CreadoPor = creadoPor },
            tx);
    }

    public async Task<int> InsertarRecetaCristalesAsync(SqlConnection con, SqlTransaction tx, NuevaRecetaCristales r, int creadoPor)
    {
        return await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO dbo.OPT_RecetaCristales
                (ClienteId,
                 OdEsferaLejos, OdCilindroLejos, OdEjeLejos, OdEsferaCerca, OdCilindroCerca, OdEjeCerca,
                 OiEsferaLejos, OiCilindroLejos, OiEjeLejos, OiEsferaCerca, OiCilindroCerca, OiEjeCerca,
                 Urgente, RequiereLab, Observaciones, DpLejos, DpCerca, AddLejos, CreadoEn, CreadoPor)
            OUTPUT INSERTED.RecetaCristalesId
            VALUES
                (@ClienteId,
                 @OdEsferaLejos, @OdCilindroLejos, @OdEjeLejos, @OdEsferaCerca, @OdCilindroCerca, @OdEjeCerca,
                 @OiEsferaLejos, @OiCilindroLejos, @OiEjeLejos, @OiEsferaCerca, @OiCilindroCerca, @OiEjeCerca,
                 @Urgente, @RequiereLab, @Observaciones, @DpLejos, @DpCerca, @AddLejos, @CreadoEn, @CreadoPor);
            """,
            new
            {
                r.ClienteId,
                r.OdEsferaLejos, r.OdCilindroLejos, r.OdEjeLejos, r.OdEsferaCerca, r.OdCilindroCerca, r.OdEjeCerca,
                r.OiEsferaLejos, r.OiCilindroLejos, r.OiEjeLejos, r.OiEsferaCerca, r.OiCilindroCerca, r.OiEjeCerca,
                r.Urgente, r.RequiereLab, r.Observaciones, r.DpLejos, r.DpCerca, r.AddLejos, r.CreadoEn,
                CreadoPor = creadoPor
            },
            tx);
    }
}
