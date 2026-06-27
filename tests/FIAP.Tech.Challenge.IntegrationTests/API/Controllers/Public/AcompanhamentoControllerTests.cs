using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Public;

public class AcompanhamentoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AcompanhamentoControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FluxoPublico_CriarEObterOrdemServico_DeveRetornarCriadoERecuperarCorretamente()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CriarOrdemServicoRequest
        {
            ClienteNome = "Guilherme Santos",
            ClienteCpf = "12345678909",
            ClienteEmail = "gui@email.com",
            ClienteTelefone = "11988887777",
            VeiculoPlaca = "ABC-1234",
            VeiculoMarca = "Fiat",
            VeiculoModelo = "Uno",
            VeiculoAno = 2015,
            DescricaoProblema = "Barulho na suspensão"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act - 1. Criar OS
        var postResponse = await client.PostAsync("/api/public/Acompanhamento", content);

        // Assert post
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var postResultString = await postResponse.Content.ReadAsStringAsync();
        var osCriada = JsonSerializer.Deserialize<OrdemServicoResponse>(postResultString, _jsonOptions);
        osCriada.Should().NotBeNull();
        osCriada!.Id.Should().NotBeEmpty();
        osCriada.Status.Should().Be("Recebida");

        // Act - 2. Obter OS por ID
        var getResponse = await client.GetAsync($"/api/public/Acompanhamento/{osCriada.Id}");

        // Assert get
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResultString = await getResponse.Content.ReadAsStringAsync();
        var osRecuperada = JsonSerializer.Deserialize<OrdemServicoResponse>(getResultString, _jsonOptions);
        osRecuperada.Should().NotBeNull();
        osRecuperada!.Id.Should().Be(osCriada.Id);
        osRecuperada.DescricaoProblema.Should().Be("Barulho na suspensão");
    }
}
