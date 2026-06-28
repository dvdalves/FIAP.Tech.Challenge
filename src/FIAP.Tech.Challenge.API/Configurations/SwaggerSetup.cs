using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.OpenApi;

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
                    new List<string>()
                }
            });

            // Carrega os comentários XML para exibir resumos e descrições no Swagger
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
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