namespace OPT.Application.Features.Cobranza;

/// <summary>
/// Fila del listado de deudores: una empresa convenio con su deuda vigente consolidada.
/// Reemplaza al <c>sp_ListaDeudores</c> del legacy, que además calculaba el saldo restando
/// en la vista (<c>Saldo - Pagado</c>); acá el saldo ya viene recalculado por el agregado OT.
///
/// <see cref="EmpresaPublicId"/> es null en la fila de OT particulares (sin empresa convenio):
/// la empresa se direcciona por PublicId, nunca por su Id interno (ADR 0004).
/// </summary>
public record DeudorEmpresaDto(
    Guid?   EmpresaPublicId,
    string  EmpresaNombre,
    string? EmpresaRut,
    int     CantidadOT,
    decimal TotalPrecio,
    decimal TotalAbonado,
    decimal Saldo);
