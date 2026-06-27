using System.Data.Common;
using System.Linq;
using FIAP.Tech.Challenge.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;

namespace FIAP.Tech.Challenge.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Remover TODAS as configurações originais do DbContext e DbContextOptions para evitar conflitos
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<OficinaDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(OficinaDbContext)
            ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // 2. Criar e abrir uma conexão SQLite em memória dedicada para este Factory
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // 3. Registrar a conexão aberta como Singleton para mantê-la ativa durante a vida do teste
            services.AddSingleton<DbConnection>(connection);

            // 4. Registrar o DbContext usando a conexão em memória
            services.AddDbContext<OficinaDbContext>((container, options) =>
            {
                options.UseSqlite(connection);
            });

            // 5. Adicionar a Assembly do Program explicitamente para descoberta de Controllers no ambiente de teste
            services.AddControllers()
                .AddApplicationPart(typeof(Program).Assembly);

            // 6. Garantir a criação das tabelas no banco de dados em memória
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<OficinaDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
