using OPT.API.Middleware;
using OPT.Application;
using OPT.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Capas ─────────────────────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── API ────────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OPT API", Version = "v1" });

    // Soporte de JWT en Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Ingrese el token JWT obtenido en /api/auth/login"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────────
// 1. Manejo centralizado de excepciones — SIEMPRE primero
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Microsoft.OpenApi >= 1.6.14 (forzado por Swashbuckle.AspNetCore.Swagger desde su propia
    // 6.6.2, no hay forma de fijar una versión anterior sin degradar el paquete) serializa el
    // documento v3 con "openapi":"3.0.4" en vez de "3.0.1". El swagger-ui-dist que Swashbuckle
    // sigue embebiendo (verificado hasta su 7.2.0) todavía valida la versión con la expresión
    // regular /^3\.0\.([0123])(?:-rc[012])?$/, que no incluye "4" -- sin este parche, Swagger UI
    // muestra "Unable to render this definition" aunque el swagger.json es válido. Se reescribe
    // el string servido solo para ese documento; no cambia el modelo generado ni afecta a otros
    // consumidores del esquema. Quitar cuando Swashbuckle.AspNetCore actualice su swagger-ui-dist.
    app.Use(async (context, next) =>
    {
        if (context.Request.Path != "/swagger/v1/swagger.json")
        {
            await next();
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        await next();

        buffer.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(buffer).ReadToEndAsync();
        json = json.Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.1\"");

        context.Response.Body = originalBody;
        context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(json);
        await context.Response.WriteAsync(json);
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
