using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.API.Configurations;

[ExcludeFromCodeCoverage]
public static class SwaggerSetup
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Oficina Mecânica API", 
                Version = "v1",
                Description = "API de gerenciamento e atendimento de ordens de serviço de uma oficina mecânica."
            });

            // Configuração do esquema de autenticação JWT Bearer no Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Insira o token JWT gerado (o prefixo 'Bearer ' será adicionado automaticamente).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc),
                    new System.Collections.Generic.List<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oficina Mecânica API v1");
            c.RoutePrefix = "swagger"; // Acessível em /swagger
        });

        return app;
    }
}
