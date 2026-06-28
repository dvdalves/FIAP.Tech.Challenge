using System.Net;
using System.Net.Http.Headers;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
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

    [Fact]
    public async Task ObterTodos_ComVeiculos_DeveRetornarVeiculosNoJson()
    {
        // Arrange
        var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            
            // Clean up database for isolated test
            context.Clientes.RemoveRange(context.Clientes);
            context.Veiculos.RemoveRange(context.Veiculos);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var cliente = new Cliente(Guid.NewGuid(), "José Teste", new Cpf("09876543229"), "jose@teste.com", "11988887777");
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);
            
            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("ABC1234"), "Ford", "Ka", 2018, cliente.Id);
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);
            
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
            var token = tokenService.GerarToken("teste_admin", "Admin");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Act
        var response = await client.GetAsync("/api/admin/Clientes", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("José Teste");
        content.Should().Contain("ABC1234");
        content.Should().Contain("veiculos");
    }
}