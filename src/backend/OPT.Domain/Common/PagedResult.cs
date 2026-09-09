namespace OPT.Domain.Common;

/// <summary>
/// Página de resultados de una consulta de listado. Forma única reutilizada por todos los
/// endpoints paginados (Clientes, Empresas, Usuarios, Sucursales, y los futuros
/// OrdenesDeTrabajo / Inventario). Los tres últimos miembros son calculados — el cliente
/// no necesita derivarlos y se serializan igual que una propiedad normal.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Pagina, int TamanioPagina, int Total)
{
    public int TotalPaginas =>
        TamanioPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanioPagina);

    public bool TienePaginaAnterior => Pagina > 1;

    public bool TienePaginaSiguiente => Pagina < TotalPaginas;
}

/// <summary>
/// Ensambla la página final de DTOs desde el resultado crudo del repositorio
/// (entidades + total), evitando repetir el <c>.Select(...).ToList()</c> + constructor en
/// cada handler de listado.
/// </summary>
public static class PagedResultFactory
{
    public static PagedResult<TDto> Crear<TEntidad, TDto>(
        IReadOnlyList<TEntidad> items, int total, ParametrosPaginacion parametros, Func<TEntidad, TDto> map)
        => new(items.Select(map).ToList(), parametros.Pagina, parametros.TamanioPagina, total);
}
