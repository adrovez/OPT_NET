using MediatR;
using OPT.Application.Features.RecetaCristales;

namespace OPT.Application.Features.RecetaCristales.Commands.Crear;

public record CrearRecetaCristalesCommand(
    Guid ClientePublicId,
    decimal? OdEsferaLejos, decimal? OdCilindroLejos, int? OdEjeLejos,
    decimal? OdEsferaCerca, decimal? OdCilindroCerca, int? OdEjeCerca,
    decimal? OiEsferaLejos, decimal? OiCilindroLejos, int? OiEjeLejos,
    decimal? OiEsferaCerca, decimal? OiCilindroCerca, int? OiEjeCerca,
    bool Urgente, bool RequiereLab, string? Observaciones,
    string? DpLejos, string? DpCerca, string? AddLejos,
    bool IncluirLejos, string? ObservacionOdLejos, string? ObservacionOiLejos, string? ObservacionDpLejos,
    bool IncluirCerca, string? ObservacionOdCerca, string? ObservacionOiCerca, string? ObservacionDpCerca)
    : IRequest<RecetaCristalesDto>;
