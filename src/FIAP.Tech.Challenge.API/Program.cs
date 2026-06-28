using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.API.Configurations;
using FIAP.Tech.Challenge.API.Filters;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar injeção de dependências e Banco de Dados (EF Core + SQLite)
builder.Services.AddDependencyInjection(builder.Configuration);

// 2. Configurar autenticação JWT Bearer
builder.Services.AddJwtAuthentication();

// 3. Configurar Controllers com Filtro de Exceção Global do Domínio
builder.Services.AddControllers(options => { options.Filters.Add<FiltroExcecaoGlobal>(); });

// 4. Configurar Swagger com esquema funcional de autenticação JWT Bearer
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// 5. Garantir a criação automática do banco SQLite para o MVP rodar imediatamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
    context.Database.EnsureCreated();
}

// 6. Configurar pipeline do HTTP request
if (app.Environment.IsDevelopment()) app.UseSwaggerConfiguration();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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