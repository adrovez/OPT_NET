namespace OPT.Domain.Entities.Organizacion;

/// <summary>
/// Asociación muchos-a-muchos entre Empresa y Sucursal.
/// Una empresa puede operar una o más sucursales.
/// </summary>
public class EmpresaSucursal
{
    public int      Id         { get; private set; }
    public int      EmpresaId  { get; private set; }
    public int      SucursalId { get; private set; }
    public Empresa?  Empresa   { get; private set; }
    public Sucursal? Sucursal  { get; private set; }

    protected EmpresaSucursal() { }

    public EmpresaSucursal(int empresaId, int sucursalId)
    {
        EmpresaId  = empresaId;
        SucursalId = sucursalId;
    }
}
