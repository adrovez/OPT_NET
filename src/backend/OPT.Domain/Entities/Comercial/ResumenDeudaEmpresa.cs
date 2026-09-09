namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Agregación de la deuda vigente por empresa convenio: reemplaza al <c>sp_ListaDeudores</c>
/// del legacy. No es una entidad — es el resultado de una consulta de agrupación sobre
/// <see cref="OrdenDeTrabajo"/>, y por eso no hereda de <c>AuditableEntity</c>.
///
/// <see cref="EmpresaId"/> es null para las OT particulares (sin empresa convenio); el
/// frontend las muestra agrupadas como "Sin empresa".
/// </summary>
public record ResumenDeudaEmpresa(
    int?    EmpresaId,
    int     CantidadOT,
    decimal TotalPrecio,
    decimal TotalAbonado,
    decimal Saldo);
