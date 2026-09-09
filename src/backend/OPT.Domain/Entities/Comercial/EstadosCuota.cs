namespace OPT.Domain.Entities.Comercial;

/// <summary>
/// Ids fijos del catálogo <c>OPT_EstadoCuota</c> (script <c>004_comercial_pagos_cuotas.sql</c>).
/// El legacy guardaba el estado de la cuota como texto libre — aquí es un catálogo (ADR 0006).
/// </summary>
public static class EstadosCuota
{
    public const int Pendiente = 1;
    public const int Pagada    = 2;
    public const int Anulada   = 3;
}
