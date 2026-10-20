using OPT.Domain.Entities.Inventario;

namespace OPT.Application.Features.Bodegas;

public record BodegaDto(
    Guid PublicId, int SucursalId, string Codigo, string Nombre, bool EsPrincipal, bool Activa)
{
    public static BodegaDto Desde(Bodega b)
        => new(b.PublicId, b.SucursalId, b.Codigo, b.Nombre, b.EsPrincipal, b.Activa);
}
