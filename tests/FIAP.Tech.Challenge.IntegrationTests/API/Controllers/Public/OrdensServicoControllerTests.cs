using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.API.Controllers.Admin;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Public;

public class OrdensServicoControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private async Task<string> ObterTokenBearerAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/public/auth/token?usuario=admin&perfil=Admin", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task FluxoCompletoOrdemServico_AprovadoPeloCliente_DeveProcessarEstoqueECorretasTransicoes()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Step 1: Cadastrar Cliente
        var clienteRequest = new CriarClienteRequest
        {
            Nome = "Rodrigo Silva",
            Cpf = "12345678909",
            Email = "rodrigo@email.com",
            Telefone = "11988884444"
        };
        var clienteContent = new StringContent(JsonSerializer.Serialize(clienteRequest), Encoding.UTF8, "application/json");
        var clientPostResponse = await client.PostAsync("/api/admin/clientes", clienteContent);
        clientPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var clienteResponse = JsonSerializer.Deserialize<ClienteResponse>(
            await clientPostResponse.Content.ReadAsStringAsync(), _jsonOptions);
        clienteResponse.Should().NotBeNull();
        clienteResponse!.Id.Should().NotBeEmpty();

        // Step 2: Cadastrar Veículo
        var veiculoRequest = new CriarVeiculoRequest
        {
            Placa = "XYZ-9876",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2020
        };
        var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoRequest), Encoding.UTF8, "application/json");
        var veiculoPostResponse = await client.PostAsync($"/api/admin/clientes/{clienteResponse.Id}/veiculos", veiculoContent);
        veiculoPostResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var veiculoResponse = JsonSerializer.Deserialize<VeiculoResponse>(
            await veiculoPostResponse.Content.ReadAsStringAsync(), _jsonOptions);
        veiculoResponse.Should().NotBeNull();
        veiculoResponse!.Id.Should().NotBeEmpty();

        // Step 3: Atendente abre Ordem de Serviço (Status: Recebida)
        var abrirOSRequest = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteResponse.Id,
            VeiculoId = veiculoResponse.Id,
            DescricaoProblema = "Revisão geral e pastilha desgastada"
        };
        var abrirOSContent = new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var osPostResponse = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent);
        osPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var osResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await osPostResponse.Content.ReadAsStringAsync(), _jsonOptions);
        osResponse.Should().NotBeNull();
        osResponse!.Status.Should().Be("Recebida");

        // Step 4: Mecânico realiza diagnóstico e adiciona itens (Status muda para AguardandoAprovacao)
        // Adiciona 2 Pastilhas de Freio (ID: 22222222-2222-2222-2222-222222222222, Preço: 180.00, Estoque Inicial: 8)
        var diagnosticoRequest = new LancarItensOSRequest
        {
            Pecas = new List<PecaItemRequest>
            {
                new PecaItemRequest { PecaId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Quantidade = 2 }
            },
            Servicos = new List<ServicoItemRequest>
            {
                new ServicoItemRequest { Descricao = "Substituição de pastilhas dianteiras", ValorMaoDeObra = 90.00m }
            }
        };
        var diagnosticoContent = new StringContent(JsonSerializer.Serialize(diagnosticoRequest), Encoding.UTF8, "application/json");
        var itensPostResponse = await client.PostAsync($"/api/admin/ordens-servico/{osResponse.Id}/itens", diagnosticoContent);
        if (itensPostResponse.StatusCode != HttpStatusCode.OK)
        {
            var err = await itensPostResponse.Content.ReadAsStringAsync();
            throw new Exception($"LancarItens failed: {itensPostResponse.StatusCode} - {err}");
        }
        itensPostResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osDiagnosticoResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await itensPostResponse.Content.ReadAsStringAsync(), _jsonOptions);
        osDiagnosticoResponse.Should().NotBeNull();
        osDiagnosticoResponse!.Status.Should().Be("AguardandoAprovacao");
        // Orçamento calculado automaticamente: (180.00 * 2) + 90.00 = 450.00
        osDiagnosticoResponse.ValorTotal.Should().Be(450.00m);

        // Step 5: Cliente aprova o orçamento (Status muda para EmExecucao e abate o estoque)
        // Removemos a autorização Bearer para simular o cliente acessando publicamente pelo App
        client.DefaultRequestHeaders.Authorization = null;
        var aprovarResponse = await client.PostAsync($"/api/public/ordens-servico/{osResponse.Id}/aprovar", null);
        aprovarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osAprovadaResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await aprovarResponse.Content.ReadAsStringAsync(), _jsonOptions);
        osAprovadaResponse.Should().NotBeNull();
        osAprovadaResponse!.Status.Should().Be("EmExecucao");

        // Step 6: Verificar se o estoque foi deduzido (de 8 para 6 pastilhas)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var pecasGetResponse = await client.GetAsync("/api/admin/pecas");
        pecasGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pecas = JsonSerializer.Deserialize<List<PecaResponse>>(
            await pecasGetResponse.Content.ReadAsStringAsync(), _jsonOptions);
        pecas.Should().NotBeNull();
        var pastilha = pecas!.Find(p => p.Id == Guid.Parse("22222222-2222-2222-2222-222222222222"));
        pastilha.Should().NotBeNull();
        pastilha!.QuantidadeEstoque.Should().Be(6); // 8 inicial - 2 deduzidas
    }

    [Fact]
    public async Task RejeitarOrcamento_PeloCliente_DeveTransitarParaCancelada()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Criar Cliente
        var clienteRequest = new CriarClienteRequest { Nome = "Maria Souza", Cpf = "11122233396", Email = "maria@email.com", Telefone = "11977776666" };
        var clienteContent = new StringContent(JsonSerializer.Serialize(clienteRequest), Encoding.UTF8, "application/json");
        var clientPostResponse = await client.PostAsync("/api/admin/clientes", clienteContent);
        var clienteResponse = JsonSerializer.Deserialize<ClienteResponse>(await clientPostResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Criar Veículo
        var veiculoRequest = new CriarVeiculoRequest { Placa = "DEF-5678", Marca = "Ford", Modelo = "Ka", Ano = 2018 };
        var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoRequest), Encoding.UTF8, "application/json");
        var veiculoPostResponse = await client.PostAsync($"/api/admin/clientes/{clienteResponse!.Id}/veiculos", veiculoContent);
        var veiculoResponse = JsonSerializer.Deserialize<VeiculoResponse>(await veiculoPostResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Abrir OS
        var abrirOSRequest = new AbrirOrdemServicoRequest { ClienteId = clienteResponse.Id, VeiculoId = veiculoResponse!.Id, DescricaoProblema = "Vazamento de água" };
        var abrirOSContent = new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var osPostResponse = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent);
        var osResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(await osPostResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Mecânico adiciona diagnóstico
        var diagnosticoRequest = new LancarItensOSRequest
        {
            Servicos = new List<ServicoItemRequest> { new ServicoItemRequest { Descricao = "Substituição do reservatório", ValorMaoDeObra = 150.00m } }
        };
        var diagnosticoContent = new StringContent(JsonSerializer.Serialize(diagnosticoRequest), Encoding.UTF8, "application/json");
        await client.PostAsync($"/api/admin/ordens-servico/{osResponse!.Id}/itens", diagnosticoContent);

        // Act: Cliente rejeita
        client.DefaultRequestHeaders.Authorization = null;
        var rejeitarResponse = await client.PostAsync($"/api/public/ordens-servico/{osResponse.Id}/rejeitar", null);
        rejeitarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osRejeitada = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await rejeitarResponse.Content.ReadAsStringAsync(), _jsonOptions);
        osRejeitada.Should().NotBeNull();
        osRejeitada!.Status.Should().Be("Cancelada");
    }

    [Fact]
    public async Task AbrirOrdemServico_ComVeiculoDeOutroCliente_DeveRetornarBadRequest()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Criar Cliente A
        var clienteARequest = new CriarClienteRequest { Nome = "Cliente A", Cpf = "22233344405", Email = "clientea@email.com", Telefone = "11911111111" };
        var clienteAContent = new StringContent(JsonSerializer.Serialize(clienteARequest), Encoding.UTF8, "application/json");
        var postClienteAResponse = await client.PostAsync("/api/admin/clientes", clienteAContent);
        var clienteA = JsonSerializer.Deserialize<ClienteResponse>(await postClienteAResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Criar Veiculo A para Cliente A
        var veiculoARequest = new CriarVeiculoRequest { Placa = "AAA-1234", Marca = "Chevrolet", Modelo = "Onix", Ano = 2019 };
        var veiculoAContent = new StringContent(JsonSerializer.Serialize(veiculoARequest), Encoding.UTF8, "application/json");
        await client.PostAsync($"/api/admin/clientes/{clienteA!.Id}/veiculos", veiculoAContent);

        // Criar Cliente B
        var clienteBRequest = new CriarClienteRequest { Nome = "Cliente B", Cpf = "33344455508", Email = "clienteb@email.com", Telefone = "11922222222" };
        var clienteBContent = new StringContent(JsonSerializer.Serialize(clienteBRequest), Encoding.UTF8, "application/json");
        var postClienteBResponse = await client.PostAsync("/api/admin/clientes", clienteBContent);
        var clienteB = JsonSerializer.Deserialize<ClienteResponse>(await postClienteBResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Criar Veiculo B para Cliente B
        var veiculoBRequest = new CriarVeiculoRequest { Placa = "BBB-5678", Marca = "Fiat", Modelo = "Uno", Ano = 2015 };
        var veiculoBContent = new StringContent(JsonSerializer.Serialize(veiculoBRequest), Encoding.UTF8, "application/json");
        var postVeiculoBResponse = await client.PostAsync($"/api/admin/clientes/{clienteB!.Id}/veiculos", veiculoBContent);
        var veiculoB = JsonSerializer.Deserialize<VeiculoResponse>(await postVeiculoBResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Act: Tenta abrir OS para Cliente A com Veiculo B
        var abrirOSRequest = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteA.Id,
            VeiculoId = veiculoB!.Id,
            DescricaoProblema = "Problema com veiculo de terceiro"
        };
        var abrirOSContent = new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Este veículo não pertence ao cliente informado.");
    }

    [Fact]
    public async Task AtualizarStatus_ViaAdminEndpoint_DeveTransitarStatusCorretamente()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Criar Cliente
        var clienteRequest = new CriarClienteRequest { Nome = "Maria Silva", Cpf = "44455566619", Email = "maria.silva@email.com", Telefone = "11977778888" };
        var clienteContent = new StringContent(JsonSerializer.Serialize(clienteRequest), Encoding.UTF8, "application/json");
        var postClienteResponse = await client.PostAsync("/api/admin/clientes", clienteContent);
        var cliente = JsonSerializer.Deserialize<ClienteResponse>(await postClienteResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Criar Veiculo
        var veiculoRequest = new CriarVeiculoRequest { Placa = "CCC-1234", Marca = "Chevrolet", Modelo = "Cruze", Ano = 2021 };
        var veiculoContent = new StringContent(JsonSerializer.Serialize(veiculoRequest), Encoding.UTF8, "application/json");
        var postVeiculoResponse = await client.PostAsync($"/api/admin/clientes/{cliente!.Id}/veiculos", veiculoContent);
        var veiculo = JsonSerializer.Deserialize<VeiculoResponse>(await postVeiculoResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Abrir OS
        var abrirOSRequest = new AbrirOrdemServicoRequest { ClienteId = cliente.Id, VeiculoId = veiculo!.Id, DescricaoProblema = "Troca de amortecedor" };
        var abrirOSContent = new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var postOSResponse = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent);
        var os = JsonSerializer.Deserialize<OrdemServicoResponse>(await postOSResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Act: Atualizar status para EmDiagnostico usando JSON request body
        var statusRequest = new { NovoStatus = (int)StatusOrdemServico.EmDiagnostico };
        var statusContent = new StringContent(JsonSerializer.Serialize(statusRequest), Encoding.UTF8, "application/json");
        var putResponse = await client.PutAsync($"/api/admin/ordens-servico/{os!.Id}/status", statusContent);
        
        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAtualizada = JsonSerializer.Deserialize<OrdemServicoResponse>(await putResponse.Content.ReadAsStringAsync(), _jsonOptions);
        osAtualizada.Should().NotBeNull();
        osAtualizada!.Status.Should().Be("EmDiagnostico");
    }
}
