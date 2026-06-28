using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
        var response = await client.GetAsync("/api/admin/pecas", TestContext.Current.CancellationToken);

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
        var response = await client.GetAsync("/api/admin/pecas", TestContext.Current.CancellationToken);

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
        var response = await client.PostAsJsonAsync("/api/admin/pecas", request, cancellationToken: TestContext.Current.CancellationToken);

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
        var response = await client.PostAsJsonAsync("/api/admin/pecas", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task FluxoCRUD_Peca_DeveFuncionarCorretamente()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());

        // 1. Criar
        var requestCriar = new AdicionarPecaRequest { Nome = "Peca Integracao", Preco = 100.00m, QuantidadeEstoque = 10 };
        var responseCriar = await client.PostAsJsonAsync("/api/admin/pecas", requestCriar, TestContext.Current.CancellationToken);
        responseCriar.StatusCode.Should().Be(HttpStatusCode.Created);
        var peca = await responseCriar.Content.ReadFromJsonAsync<PecaResponse>(cancellationToken: TestContext.Current.CancellationToken);
        peca.Should().NotBeNull();

        // 2. Obter por ID
        var responseObter = await client.GetAsync($"/api/admin/pecas/{peca!.Id}", TestContext.Current.CancellationToken);
        responseObter.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Atualizar
        var requestAtualizar = new AtualizarPecaRequest { Nome = "Peca Integracao Atualizada", Preco = 120.00m };
        var responseAtualizar = await client.PutAsJsonAsync($"/api/admin/pecas/{peca.Id}", requestAtualizar, TestContext.Current.CancellationToken);
        responseAtualizar.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Excluir
        var responseExcluir = await client.DeleteAsync($"/api/admin/pecas/{peca.Id}", TestContext.Current.CancellationToken);
        responseExcluir.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Verificar exclusao
        var responseObterExcluida = await client.GetAsync($"/api/admin/pecas/{peca.Id}", TestContext.Current.CancellationToken);
        responseObterExcluida.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}