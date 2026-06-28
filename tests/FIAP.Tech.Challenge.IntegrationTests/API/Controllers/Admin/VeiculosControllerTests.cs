using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Admin;

public class VeiculosControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private string ObterTokenAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        return tokenService.GerarToken("teste_admin", "Admin");
    }

    [Fact]
    public async Task ObterTodos_SemAutenticacao_DeveRetornar401Unauthorized()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/veiculos", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FluxoCRUDVeiculo_DeveFuncionarCorretamente()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ObterTokenAdmin());

        Guid clienteId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            
            // Clean up to isolate test
            context.OrdensServico.RemoveRange(context.OrdensServico);
            context.Veiculos.RemoveRange(context.Veiculos);
            context.Clientes.RemoveRange(context.Clientes);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var cliente = new Cliente(Guid.NewGuid(), "Cliente Veiculo", new Cpf("98765432100"), "cliente.veiculo@email.com", "11988887777");
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            clienteId = cliente.Id;
        }

        // 1. Criar Veiculo via endpoint existente
        var request = new CriarVeiculoRequest
        {
            Placa = "XYZ9D87",
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2020
        };

        var responseCriar = await client.PostAsJsonAsync($"/api/admin/clientes/{clienteId}/veiculos", request, TestContext.Current.CancellationToken);
        responseCriar.StatusCode.Should().Be(HttpStatusCode.OK);
        var veiculoCriado = await responseCriar.Content.ReadFromJsonAsync<VeiculoResponse>(cancellationToken: TestContext.Current.CancellationToken);
        veiculoCriado.Should().NotBeNull();
        veiculoCriado!.Placa.Should().Be("XYZ9D87");

        // 2. Obter por ID
        var responseObter = await client.GetAsync($"/api/admin/veiculos/{veiculoCriado.Id}", TestContext.Current.CancellationToken);
        responseObter.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Atualizar
        var requestAtualizar = new AtualizarVeiculoRequest
        {
            Placa = "XYZ9D87",
            Marca = "Ford",
            Modelo = "Ka Sedan",
            Ano = 2021
        };
        var responseAtualizar = await client.PutAsJsonAsync($"/api/admin/veiculos/{veiculoCriado.Id}", requestAtualizar, TestContext.Current.CancellationToken);
        responseAtualizar.StatusCode.Should().Be(HttpStatusCode.OK);
        var veiculoAtualizado = await responseAtualizar.Content.ReadFromJsonAsync<VeiculoResponse>(cancellationToken: TestContext.Current.CancellationToken);
        veiculoAtualizado!.Modelo.Should().Be("Ka Sedan");
        veiculoAtualizado.Ano.Should().Be(2021);

        // 4. Listar Todos
        var responseListar = await client.GetAsync("/api/admin/veiculos", TestContext.Current.CancellationToken);
        responseListar.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await responseListar.Content.ReadFromJsonAsync<IEnumerable<Veiculo>>(cancellationToken: TestContext.Current.CancellationToken);
        lista.Should().Contain(v => v.Id == veiculoCriado.Id);

        // 5. Excluir
        var responseExcluir = await client.DeleteAsync($"/api/admin/veiculos/{veiculoCriado.Id}", TestContext.Current.CancellationToken);
        responseExcluir.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verificar que foi excluido (Obter por ID retorna 404)
        var responseObterExcluido = await client.GetAsync($"/api/admin/veiculos/{veiculoCriado.Id}", TestContext.Current.CancellationToken);
        responseObterExcluido.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
