namespace OPT.Domain.Entities.Inventario;

/// <summary>
/// Ids fijos de <see cref="CategoriaProducto"/> (sembrados en el script 014 y en
/// <c>CategoriaProductoConfiguration</c>) y la regla que depende de ellos (ADR 0013).
/// </summary>
public static class CategoriaProductoIds
{
    public const int General      = 1;
    public const int Marcos       = 2;
    public const int Cristales    = 3;
    public const int LentesDeSol  = 4;
    public const int Otros        = 5;

    /// <summary>
    /// Marcos y Lentes de Sol se identifican por Marca + Modelo (Color es opcional) y su
    /// Descripción se calcula a partir de ellos; el resto de las categorías usa descripción libre.
    /// </summary>
    public static bool ExigeMarcaYModelo(int categoriaId)
        => categoriaId is Marcos or LentesDeSol;
}
