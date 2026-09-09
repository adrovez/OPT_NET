namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Asociación muchos-a-muchos entre Usuario y Sucursal.
/// Un usuario puede pertenecer a una o más sucursales.
/// </summary>
public class UsuarioSucursal
{
    public int      Id         { get; private set; }
    public int      UsuarioId  { get; private set; }
    public int      SucursalId { get; private set; }
    public Usuario?  Usuario   { get; private set; }
    public Sucursal? Sucursal  { get; private set; }

    protected UsuarioSucursal() { }

    public UsuarioSucursal(int usuarioId, int sucursalId)
    {
        UsuarioId  = usuarioId;
        SucursalId = sucursalId;
    }
}
