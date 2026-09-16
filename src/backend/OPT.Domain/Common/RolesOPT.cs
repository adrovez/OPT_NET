namespace OPT.Domain.Common;

/// <summary>
/// Los 8 roles reales sembrados en <c>OPT_Rol</c> (3 genéricos del scaffold + 5 heredados
/// del legacy, ver script <c>002_catalogos.sql</c>). El legacy no restringía ninguna acción
/// por rol a nivel de servidor — solo ocultaba ítems de menú (ver <c>_ParcialMenu.cshtml</c>) —
/// así que estos grupos son un diseño nuevo, no una migración de una regla existente.
/// </summary>
public static class RolesOPT
{
    public const int Administrador  = 1;
    public const int Supervisor     = 2;
    public const int Operador       = 3;
    public const int JefeSucursal   = 4;
    public const int Vendedor       = 5;
    public const int TecnicoMedico  = 6;
    public const int ControlCalidad = 7;
    public const int Externo        = 8;

    /// <summary>Administración de la organización (Sucursales/Usuarios/Roles) — el rol más alto.</summary>
    public static readonly int[] Administracion = [Administrador];

    /// <summary>Gestión comercial de alto nivel (p. ej. alta de Empresas convenio).</summary>
    public static readonly int[] GestionComercial = [Administrador, Supervisor];

    /// <summary>
    /// Datos clínicos (Cliente, Anamnesis, RecetaCristales — sensibles, ADR 0004): el personal
    /// que atiende al paciente o vende la orden. Control Calidad y Externo quedan afuera porque
    /// no tienen necesidad de negocio de ver/editar historial clínico.
    /// </summary>
    public static readonly int[] OperacionClinica =
        [Administrador, Supervisor, JefeSucursal, Vendedor, TecnicoMedico, Operador];

    /// <summary>Operación normal del agregado OT: crear, editar, cobrar, generar cuotas.</summary>
    public static readonly int[] OperacionComercial =
        [Administrador, Supervisor, JefeSucursal, Vendedor, Operador];

    /// <summary>Igual que <see cref="OperacionComercial"/> más Control Calidad, para el paso de
    /// estado por la etapa CALIDAD del flujo de la OT.</summary>
    public static readonly int[] OperacionComercialConCalidad =
        [Administrador, Supervisor, JefeSucursal, Vendedor, Operador, ControlCalidad];

    /// <summary>Anular una OT o una cuota es de mayor impacto financiero — se reserva a supervisión.</summary>
    public static readonly int[] AnulacionComercial = [Administrador, Supervisor, JefeSucursal];

    /// <summary>Reportes de cobranza/deudores.</summary>
    public static readonly int[] Cobranza = [Administrador, Supervisor, JefeSucursal];

    /// <summary>
    /// Roles que pueden operar recursos de cualquier sucursal sin la validación BOLA de
    /// <c>AutorizacionSucursal</c> — hoy solo Administrador (alcance nacional).
    /// </summary>
    public static readonly int[] AccesoTotalSucursales = [Administrador];
}
