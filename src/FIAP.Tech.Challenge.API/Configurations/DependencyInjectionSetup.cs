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
        // Banco de dados (usando SQLite, PostgreSQL ou InMemory detectado automaticamente ou via configuração)
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
            else if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase(connectionString);
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
        services.AddScoped<FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate.IPecaRepository, PecaRepository>();

        // Casos de Uso
        services.AddScoped<CriarOrdemServicoUseCase>();
        services.AddScoped<AtualizarStatusOSUseCase>();
        services.AddScoped<FIAP.Tech.Challenge.Application.UseCases.Clientes.CriarClienteUseCase>();
        services.AddScoped<FIAP.Tech.Challenge.Application.UseCases.Clientes.CriarVeiculoUseCase>();
        services.AddScoped<AbrirOrdemServicoUseCase>();
        services.AddScoped<LancarItensOSUseCase>();
        services.AddScoped<AprovarOrcamentoUseCase>();
        services.AddScoped<RejeitarOrcamentoUseCase>();
        services.AddScoped<FIAP.Tech.Challenge.Application.UseCases.Pecas.AjustarEstoquePecaUseCase>();

        // Serviços de Infraestrutura
        services.AddSingleton<TokenService>();

        // Validadores do FluentValidation
        services.AddValidatorsFromAssemblyContaining<CriarOrdemServicoValidator>();

        return services;
    }
}
