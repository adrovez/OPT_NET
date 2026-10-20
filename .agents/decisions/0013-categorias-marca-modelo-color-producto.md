# 0013 — Producto: categorías reales y campos Marca / Modelo / Color

**Estado:** Aceptada
**Fecha:** 2026-10-19

## Contexto

`OPT_CategoriaProducto` tenía una sola fila ("General") y `OPT_Producto.Descripcion` guardaba, para los marcos, el texto legacy `MARCA-MODELO-COLOR` separado por guiones. El negocio pide categorías reales (Marcos, Cristales, Lentes de Sol, Otros) y campos propios para Marca, Modelo y Color.

Hallazgos sobre los 4.018 productos migrados: 4.017 llevan guiones, pero los guiones no distinguen campos (marcas compuestas como `RAY-BAN` o `ANGELO-FALCONI`, modelos con guiones como `RB-6285`). Hay ~60 productos que no son marcos (cristales, lentes de sol, líquidos, servicios como consulta o garantía).

## Decisión

1. **Categorías**: se agregan Marcos (2), Cristales (3), Lentes de Sol (4) y Otros (5). **"General" (1) se conserva** (decisión del negocio). Los ids viven en `CategoriaProductoIds` (OPT.Domain) — no en `CategoriasProducto`, que chocaría con el namespace `OPT.Application.Features.CategoriasProducto`.
2. **Columnas** `Marca`, `Modelo` (`nvarchar(100)`) y `Color` (`nvarchar(50)`), todas `NULL` en la BD. **Marca y Modelo son obligatorias solo en Marcos y Lentes de Sol**; Color es siempre opcional. La obligatoriedad la valida el dominio (`CategoriaProductoIds.ExigeMarcaYModelo`) y los validators, no un constraint.
3. **`Descripcion` calculada**: en Marcos y Lentes de Sol se arma como `Marca-Modelo[-Color]` dentro de `Producto` (lo que el cliente envíe se ignora). En el resto de las categorías sigue siendo texto libre y obligatorio. La columna no se elimina: la usan la búsqueda, el detalle de la OT y el ticket.
4. **Backfill (M007)**, solo sobre filas aún en "General": `CRISTALES…` → Cristales; contiene la palabra `SOL` → Lentes de Sol; contiene `LENTE` o empieza con `OTRO-` → Otros; el resto → Marcos. Solo los Marcos se separan en Marca/Modelo/Color. Marca = primer segmento, salvo `ANGELO`/`ANGELOFALCONI`/`ANGELO-FALCONI` → `ANGELO FALCONI` y `RAY`/`RAY-BAN`/`RAYBAN` → `RAYBAN`; `BL`, `CX`, `GEORGE`, `JEAN`, `CARLOS` y `COOL` quedan tal cual. Color = último segmento si es `C1…C99` o un color conocido.
5. API: `GET /api/categorias-producto` (solo lectura), filtro `categoriaId` y búsqueda/orden por marca, modelo y color en `GET /api/productos`.

## Consecuencias

- Resultado del backfill: 3.957 Marcos, 24 Cristales, 11 Lentes de Sol y 26 Otros; 230 marcas distintas; ningún Marco sin marca ni modelo. Muchas marcas de dos palabras quedaron cortadas en el primer segmento (`EMPORIO-ARMANI`, `LUIGI-BATANI`, `GEORGE-…`): su modelo arranca con la segunda palabra. Es la decisión "tal cual" del negocio; se corrige editando el producto.
- Los Lentes de Sol migrados (11) no se parsearon: tienen Marca/Modelo vacíos y, al editarlos, el formulario los exigirá.
- Los modelos conservan los guiones dobles del legacy (`RB-3539--002-8G`); no se limpiaron.
- Script `014` escrito a mano; entidad, configuración y seed de EF actualizados en la misma sesión (el modelo sigue alineado).
