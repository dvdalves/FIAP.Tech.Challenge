using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.API.Configurations;
using FIAP.Tech.Challenge.API.Filters;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar injeção de dependências e banco PostgreSQL (InMemory apenas nos testes)
builder.Services.AddDependencyInjection(builder.Configuration);
builder.AddServiceDefaults();

// 2. Configurar autenticação JWT Bearer
builder.Services.AddJwtAuthentication(builder.Configuration);

// 3. Configurar Health Checks para Kubernetes liveness e readiness probes
builder.Services.AddHealthChecks();

// 4. Configurar Controllers com Filtro de Exceção Global do Domínio e Conversor de Enum
builder.Services.AddControllers(options => { options.Filters.Add<FiltroExcecaoGlobal>(); })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 5. Configurar Swagger com esquema funcional de autenticação JWT Bearer
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// 6. Aplicar automaticamente as migrations do banco
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
    if (context.Database.IsRelational())
        context.Database.Migrate();
}

// 7. Configurar pipeline do HTTP request
if (app.Environment.IsDevelopment()) app.UseSwaggerConfiguration();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 8. Health Check endpoints para K8s e Aspire
app.MapDefaultEndpoints();

app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();

namespace FIAP.Tech.Challenge.API
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
    }
}