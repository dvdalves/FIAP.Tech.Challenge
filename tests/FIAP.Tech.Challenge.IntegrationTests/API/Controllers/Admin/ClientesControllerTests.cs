using System.Net;
using System.Net.Http.Headers;
using FIAP.Tech.Challenge.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Admin;

public class ClientesControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ObterTodos_SemToken_DeveRetornar401Unauthorized()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/Clientes", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_ComTokenValido_DeveRetornar200Ok()
    {
        // Arrange
        var client = factory.CreateClient();

        // Geramos um token de teste usando o TokenService
        using (var scope = factory.Services.CreateScope())
        {
            var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
            var token = tokenService.GerarToken("teste_admin", "Admin");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Act
        var response = await client.GetAsync("/api/admin/Clientes", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}