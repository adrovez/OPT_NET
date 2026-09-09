namespace OPT.Domain.Common;

/// <summary>
/// Parámetros comunes a toda consulta de listado paginada. Se hereda en cada
/// <c>Obtener{Modulo}Query</c> (capa Application) y ASP.NET la enlaza directamente desde el
/// query string
/// (<c>?pagina=1&amp;tamanioPagina=20&amp;busqueda=texto&amp;ordenarPor=nombre&amp;direccionOrden=asc</c>).
///
/// Vive en Domain.Common (sin dependencias) porque los contratos de repositorio
/// (<c>OPT.Domain/Interfaces/Repositories</c>) la reciben como parámetro.
///
/// Los <c>init</c> sanean la entrada: <see cref="Pagina"/> nunca es menor que 1 y
/// <see cref="TamanioPagina"/> se acota a <see cref="TamanioPaginaMaximo"/> (valor fuera de
/// rango vuelve al tamaño por defecto). Así el handler y el repositorio no repiten la
/// validación.
/// </summary>
public abstract record ParametrosPaginacion
{
    /// <summary>
    /// Tope de filas por página. Fuente única — si alguna vez debe variar por ambiente,
    /// se promueve a una opción enlazada en <c>Program.cs</c> y el clamp pasa al helper
    /// <c>QueryablePaginacionExtensions.PaginarAsync</c>.
    /// </summary>
    public const int TamanioPaginaMaximo = 100;

    private const int TamanioPaginaDefecto = 20;

    private readonly int _pagina = 1;
    public int Pagina
    {
        get => _pagina;
        init => _pagina = value < 1 ? 1 : value;
    }

    private readonly int _tamanioPagina = TamanioPaginaDefecto;
    public int TamanioPagina
    {
        get => _tamanioPagina;
        init => _tamanioPagina = value is < 1 or > TamanioPaginaMaximo ? TamanioPaginaDefecto : value;
    }

    /// <summary>Texto de búsqueda tipo "Google" — un solo término contra varios campos (definidos por cada repositorio).</summary>
    public string? Busqueda { get; init; }

    /// <summary>Clave de columna a ordenar (lista blanca por entidad). Si no coincide, se usa el orden por defecto.</summary>
    public string? OrdenarPor { get; init; }

    /// <summary>"asc" (por defecto) o "desc".</summary>
    public string? DireccionOrden { get; init; }

    public bool OrdenDescendente =>
        string.Equals(DireccionOrden, "desc", StringComparison.OrdinalIgnoreCase);

    /// <summary>Cantidad de filas a saltar para llegar a la página solicitada.</summary>
    public int Saltar => (Pagina - 1) * TamanioPagina;
}
