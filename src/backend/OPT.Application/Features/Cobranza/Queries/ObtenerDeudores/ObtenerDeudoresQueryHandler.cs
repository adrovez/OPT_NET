using MediatR;
using OPT.Domain.Interfaces.Repositories;

namespace OPT.Application.Features.Cobranza.Queries.ObtenerDeudores;

public sealed class ObtenerDeudoresQueryHandler(
    IOrdenDeTrabajoRepositorio ordenRepo,
    IEmpresaRepositorio        empresaRepo)
    : IRequestHandler<ObtenerDeudoresQuery, IReadOnlyList<DeudorEmpresaDto>>
{
    /// <summary>Etiqueta de la fila que agrupa las OT particulares (sin empresa convenio).</summary>
    private const string SinEmpresa = "Particulares (sin empresa)";

    public async Task<IReadOnlyList<DeudorEmpresaDto>> Handle(
        ObtenerDeudoresQuery request, CancellationToken ct)
    {
        var resumenes = await ordenRepo.ObtenerDeudaPorEmpresaAsync(ct);
        if (resumenes.Count == 0) return [];

        // Un solo lookup de empresas para todas las filas, no uno por fila.
        var empresas = (await empresaRepo.ObtenerTodosAsync(ct)).ToDictionary(e => e.Id);

        return resumenes
            .Select(r =>
            {
                if (r.EmpresaId is null || !empresas.TryGetValue(r.EmpresaId.Value, out var empresa))
                    return new DeudorEmpresaDto(null, SinEmpresa, null,
                        r.CantidadOT, r.TotalPrecio, r.TotalAbonado, r.Saldo);

                return new DeudorEmpresaDto(empresa.PublicId, empresa.Nombre, empresa.Rut,
                    r.CantidadOT, r.TotalPrecio, r.TotalAbonado, r.Saldo);
            })
            .OrderByDescending(d => d.Saldo)
            .ToList();
    }
}
