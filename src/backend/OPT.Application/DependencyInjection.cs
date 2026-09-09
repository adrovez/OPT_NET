using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OPT.Application.Common.Behaviours;
using OPT.Application.Features.OrdenesDeTrabajo;
using System.Reflection;

namespace OPT.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Ensamblador de la vista completa de una OT — lo comparten los nueve comandos y la
        // consulta por Id del agregado Comercial; el escaneo de MediatR no lo alcanza.
        services.AddScoped<OrdenDeTrabajoDtoFactory>();

        return services;
    }
}
