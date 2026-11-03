using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Application.Validators;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Services;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;
using FIAP.Tech.Challenge.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Tech.Challenge.API.Configurations;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionSetup
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL é o banco oficial; InMemory permanece disponível apenas para testes.
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=oficina;Username=postgres;Password=postgres";
        var dbProvider = configuration["DbProvider"] ?? "PostgreSQL";

        services.AddScoped<OficinaDbContext>(_ =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<OficinaDbContext>();
            if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                optionsBuilder.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(typeof(OficinaDbContext).Assembly.FullName));
            else if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
                optionsBuilder.UseInMemoryDatabase(connectionString);
            else
                throw new InvalidOperationException($"Provedor de banco não suportado: '{dbProvider}'.");

            return dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
                ? new PostgresOficinaDbContext(optionsBuilder.Options)
                : new OficinaDbContext(optionsBuilder.Options);
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OficinaDbContext>());

        // Repositórios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IPecaRepository, PecaRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();

        // Casos de Uso
        services.AddScoped<CriarOrdemServicoUseCase>();
        services.AddScoped<AtualizarStatusOSUseCase>();
        services.AddScoped<ConsultarStatusOSUseCase>();
        services.AddScoped<ProcessarNotificacaoOrcamentoUseCase>();
        
        services.AddScoped<CriarClienteUseCase>();
        services.AddScoped<AtualizarClienteUseCase>();
        services.AddScoped<ExcluirClienteUseCase>();
        
        services.AddScoped<CriarVeiculoUseCase>();
        services.AddScoped<AtualizarVeiculoUseCase>();
        services.AddScoped<ExcluirVeiculoUseCase>();
        
        services.AddScoped<AbrirOrdemServicoUseCase>();
        services.AddScoped<LancarItensOSUseCase>();
        services.AddScoped<AprovarOrcamentoUseCase>();
        services.AddScoped<RejeitarOrcamentoUseCase>();
        
        services.AddScoped<CriarPecaUseCase>();
        services.AddScoped<AtualizarPecaUseCase>();
        services.AddScoped<ExcluirPecaUseCase>();
        services.AddScoped<AjustarEstoquePecaUseCase>();
        
        services.AddScoped<CriarServicoUseCase>();
        services.AddScoped<AtualizarServicoUseCase>();
        services.AddScoped<ExcluirServicoUseCase>();

        // Serviços de Infraestrutura
        services.AddSingleton<TokenService>();
        services.AddScoped<IServicoNotificacao, EmailNotificacaoService>();

        // Validadores do FluentValidation
        services.AddValidatorsFromAssemblyContaining<CriarOrdemServicoValidator>();

        return services;
    }
}