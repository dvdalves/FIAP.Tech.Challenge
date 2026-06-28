using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Admin;

public class ServicosControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private string ObterTokenAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        return tokenService.GerarToken("teste_admin", "Admin");
    }

    [Fact]
    public async Task ObterTodos_SemToken_DeveRetornar401Unauthorized()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/servicos", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObterTodos_ComTokenValido_DeveRetornar200Ok()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());

        // Act
        var response = await client.GetAsync("/api/admin/servicos", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cadastrar_ComDadosInvalidos_DeveRetornar400BadRequest()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());
        var request = new CriarServicoRequest { Nome = "", PrecoMaoDeObra = -50 };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/servicos", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cadastrar_ComDadosValidos_DeveRetornar201Created()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());
        var request = new CriarServicoRequest { Nome = "Alinhamento", PrecoMaoDeObra = 80.00m };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/servicos", request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task FluxoCRUD_Servico_DeveFuncionarCorretamente()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());

        // 1. Criar
        var requestCriar = new CriarServicoRequest { Nome = "Servico Integracao", PrecoMaoDeObra = 150.00m };
        var responseCriar = await client.PostAsJsonAsync("/api/admin/servicos", requestCriar, TestContext.Current.CancellationToken);
        responseCriar.StatusCode.Should().Be(HttpStatusCode.Created);
        var servico = await responseCriar.Content.ReadFromJsonAsync<ServicoResponse>(cancellationToken: TestContext.Current.CancellationToken);
        servico.Should().NotBeNull();

        // 2. Obter por ID
        var responseObter = await client.GetAsync($"/api/admin/servicos/{servico!.Id}", TestContext.Current.CancellationToken);
        responseObter.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Atualizar
        var requestAtualizar = new AtualizarServicoRequest { Nome = "Servico Integracao Atualizado", PrecoMaoDeObra = 175.00m };
        var responseAtualizar = await client.PutAsJsonAsync($"/api/admin/servicos/{servico.Id}", requestAtualizar, TestContext.Current.CancellationToken);
        responseAtualizar.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Excluir
        var responseExcluir = await client.DeleteAsync($"/api/admin/servicos/{servico.Id}", TestContext.Current.CancellationToken);
        responseExcluir.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Verificar exclusao
        var responseObterExcluido = await client.GetAsync($"/api/admin/servicos/{servico.Id}", TestContext.Current.CancellationToken);
        responseObterExcluido.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
