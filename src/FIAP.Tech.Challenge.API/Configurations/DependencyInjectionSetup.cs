using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Application.Validators;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;
using FIAP.Tech.Challenge.Infrastructure.Services;

namespace FIAP.Tech.Challenge.API.Configurations;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco de dados (usando SQLite ou PostgreSQL detectado automaticamente ou via configuração)
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=oficina.db";
        var dbProvider = configuration["DbProvider"];

        if (string.IsNullOrEmpty(dbProvider))
        {
            dbProvider = connectionString.Contains("Host=") || connectionString.Contains("Server=")
                ? "PostgreSQL"
                : "Sqlite";
        }

        services.AddDbContext<OficinaDbContext>(options =>
        {
            if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OficinaDbContext>());

        // Repositórios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();

        // Casos de Uso
        services.AddScoped<CriarOrdemServicoUseCase>();
        services.AddScoped<AtualizarStatusOSUseCase>();

        // Serviços de Infraestrutura
        services.AddSingleton<TokenService>();

        // Validadores do FluentValidation
        services.AddValidatorsFromAssemblyContaining<CriarOrdemServicoValidator>();

        return services;
    }
}
