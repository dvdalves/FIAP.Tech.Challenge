using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Admin;

public class PecasControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private string ObterTokenAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        return tokenService.GerarToken("teste_admin", "Admin");
    }

    [Fact]
    public async Task ObterEstoque_SemToken_DeveRetornar401Unauthorized()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/pecas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterEstoque_ComTokenValido_DeveRetornar200Ok()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());

        // Act
        var response = await client.GetAsync("/api/admin/pecas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdicionarPeca_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());
        var request = new AdicionarPecaRequest { Nome = "", Preco = -10, QuantidadeEstoque = 5 };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/pecas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AdicionarPeca_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());
        var request = new AdicionarPecaRequest { Nome = "Piston", Preco = 120.00m, QuantidadeEstoque = 10 };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/pecas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
