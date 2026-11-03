using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FIAP.Tech.Challenge.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DbProvider", "InMemory");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"InMemoryDbForTesting_{Guid.NewGuid()}");
        builder.UseSetting("Jwt:Key", "test-only-key-that-is-not-a-production-secret-123456");
    }
}