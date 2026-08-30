using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.API.Configurations;
using FIAP.Tech.Challenge.API.Filters;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar injeção de dependências e Banco de Dados (EF Core + PostgreSQL / SQLite / InMemory)
builder.Services.AddDependencyInjection(builder.Configuration);

// 2. Configurar autenticação JWT Bearer
builder.Services.AddJwtAuthentication();

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

// 6. Garantir a criação automática do banco para execução ágil do ambiente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
    context.Database.EnsureCreated();
}

// 7. Configurar pipeline do HTTP request
if (app.Environment.IsDevelopment()) app.UseSwaggerConfiguration();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 8. Health Check endpoint para K8s
app.MapHealthChecks("/health");

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