using System;
using FIAP.Tech.Challenge.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FIAP.Tech.Challenge.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        // Define as variáveis de ambiente antes de inicializar o host do program minimal API
        Environment.SetEnvironmentVariable("DbProvider", "InMemory");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"InMemoryDbForTesting_{Guid.NewGuid()}");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Nenhuma configuração extra necessária, o host obterá "InMemory" diretamente das variáveis de ambiente.
    }
}
