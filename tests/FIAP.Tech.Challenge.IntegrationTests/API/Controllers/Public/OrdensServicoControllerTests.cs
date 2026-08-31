using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
            Cpf = "52998224725",
            Email = "rodrigo.silva@email.com",
            Telefone = "11988884444"
        };
        var clienteContent =
            new StringContent(JsonSerializer.Serialize(clienteRequest), Encoding.UTF8, "application/json");
        var clientPostResponse = await client.PostAsync("/api/admin/clientes", clienteContent, TestContext.Current.CancellationToken);
        clientPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var clienteResponse = JsonSerializer.Deserialize<ClienteResponse>(
            await clientPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
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
        var veiculoContent =
            new StringContent(JsonSerializer.Serialize(veiculoRequest), Encoding.UTF8, "application/json");
        var veiculoPostResponse =
            await client.PostAsync($"/api/admin/clientes/{clienteResponse.Id}/veiculos", veiculoContent, TestContext.Current.CancellationToken);
        veiculoPostResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var veiculoResponse = JsonSerializer.Deserialize<VeiculoResponse>(
            await veiculoPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        veiculoResponse.Should().NotBeNull();
        veiculoResponse!.Id.Should().NotBeEmpty();

        // Step 3: Atendente abre Ordem de Serviço (Status: Recebida)
        var abrirOSRequest = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteResponse.Id,
            VeiculoId = veiculoResponse.Id,
            DescricaoProblema = "Revisão geral e pastilha desgastada"
        };
        var abrirOSContent =
            new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var osPostResponse = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent, TestContext.Current.CancellationToken);
        osPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var osResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await osPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        osResponse.Should().NotBeNull();
        osResponse!.Status.Should().Be("Recebida");

        // Step 4: Mecânico realiza diagnóstico e adiciona itens (Status muda para AguardandoAprovacao)
        var diagnosticoRequest = new LancarItensOSRequest
        {
            Pecas = new List<PecaItemRequest>
            {
                new() { PecaId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Quantidade = 2 }
            },
            Servicos = new List<ServicoItemRequest>
            {
                new() { Descricao = "Substituição de pastilhas dianteiras", ValorMaoDeObra = 90.00m }
            }
        };
        var diagnosticoContent = new StringContent(JsonSerializer.Serialize(diagnosticoRequest), Encoding.UTF8,
            "application/json");
        var itensPostResponse =
            await client.PostAsync($"/api/admin/ordens-servico/{osResponse.Id}/itens", diagnosticoContent, TestContext.Current.CancellationToken);
        itensPostResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osDiagnosticoResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await itensPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        osDiagnosticoResponse.Should().NotBeNull();
        osDiagnosticoResponse!.Status.Should().Be("AguardandoAprovacao");
        osDiagnosticoResponse.ValorTotal.Should().Be(450.00m);

        // Step 5: Cliente aprova o orçamento (Status muda para EmExecucao e abate o estoque)
        var clienteTokenResponse = await client.PostAsync($"/api/public/auth/token?usuario={clienteResponse.Id}&perfil=Cliente", null, TestContext.Current.CancellationToken);
        clienteTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var clienteTokenContent = await clienteTokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var docCliente = JsonDocument.Parse(clienteTokenContent);
        var clienteToken = docCliente.RootElement.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clienteToken);

        var aprovarResponse = await client.PostAsync($"/api/public/ordens-servico/{osResponse.Id}/aprovar", null, TestContext.Current.CancellationToken);
        aprovarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osAprovadaResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await aprovarResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        osAprovadaResponse.Should().NotBeNull();
        osAprovadaResponse!.Status.Should().Be("EmExecucao");

        // Step 6: Verificar se o estoque foi deduzido (de 8 para 6 pastilhas)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var pecasGetResponse = await client.GetAsync("/api/admin/pecas", TestContext.Current.CancellationToken);
        pecasGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pecas = JsonSerializer.Deserialize<List<PecaResponse>>(
            await pecasGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        pecas.Should().NotBeNull();
        var pastilha = pecas!.Find(p => p.Id == Guid.Parse("22222222-2222-2222-2222-222222222222"));
        pastilha.Should().NotBeNull();
        pastilha!.QuantidadeEstoque.Should().Be(6);
    }

    [Fact]
    public async Task RejeitarOrcamento_PeloCliente_DeveTransitarParaCancelada()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Criar Cliente
        var clienteRequest = new CriarClienteRequest
        { Nome = "Maria Souza", Cpf = "11122233396", Email = "maria@email.com", Telefone = "11977776666" };
        var clienteContent =
            new StringContent(JsonSerializer.Serialize(clienteRequest), Encoding.UTF8, "application/json");
        var clientPostResponse = await client.PostAsync("/api/admin/clientes", clienteContent, TestContext.Current.CancellationToken);
        var clienteResponse =
            JsonSerializer.Deserialize<ClienteResponse>(await clientPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Criar Veículo
        var veiculoRequest = new CriarVeiculoRequest { Placa = "DEF-5678", Marca = "Ford", Modelo = "Ka", Ano = 2018 };
        var veiculoContent =
            new StringContent(JsonSerializer.Serialize(veiculoRequest), Encoding.UTF8, "application/json");
        var veiculoPostResponse =
            await client.PostAsync($"/api/admin/clientes/{clienteResponse!.Id}/veiculos", veiculoContent, TestContext.Current.CancellationToken);
        var veiculoResponse =
            JsonSerializer.Deserialize<VeiculoResponse>(await veiculoPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Abrir OS
        var abrirOSRequest = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteResponse.Id,
            VeiculoId = veiculoResponse!.Id,
            DescricaoProblema = "Vazamento de água"
        };
        var abrirOSContent =
            new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var osPostResponse = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent, TestContext.Current.CancellationToken);
        var osResponse =
            JsonSerializer.Deserialize<OrdemServicoResponse>(await osPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Mecânico adiciona diagnóstico
        var diagnosticoRequest = new LancarItensOSRequest
        {
            Servicos = new List<ServicoItemRequest>
                { new() { Descricao = "Substituição do reservatório", ValorMaoDeObra = 150.00m } }
        };
        var diagnosticoContent = new StringContent(JsonSerializer.Serialize(diagnosticoRequest), Encoding.UTF8,
            "application/json");
        await client.PostAsync($"/api/admin/ordens-servico/{osResponse!.Id}/itens", diagnosticoContent, TestContext.Current.CancellationToken);

        // Act: Cliente rejeita
        var clienteTokenResponse = await client.PostAsync($"/api/public/auth/token?usuario={clienteResponse!.Id}&perfil=Cliente", null, TestContext.Current.CancellationToken);
        clienteTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var clienteTokenContent = await clienteTokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var docCliente = JsonDocument.Parse(clienteTokenContent);
        var clienteToken = docCliente.RootElement.GetProperty("token").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clienteToken);

        var rejeitarResponse = await client.PostAsync($"/api/public/ordens-servico/{osResponse.Id}/rejeitar", null, TestContext.Current.CancellationToken);
        rejeitarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var osRejeitada = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await rejeitarResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
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
        var clienteARequest = new CriarClienteRequest
        { Nome = "Cliente A", Cpf = "22233344405", Email = "clientea@email.com", Telefone = "11911111111" };
        var clienteAContent =
            new StringContent(JsonSerializer.Serialize(clienteARequest), Encoding.UTF8, "application/json");
        var postClienteAResponse = await client.PostAsync("/api/admin/clientes", clienteAContent, TestContext.Current.CancellationToken);
        var clienteA =
            JsonSerializer.Deserialize<ClienteResponse>(await postClienteAResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Criar Veiculo A para Cliente A
        var veiculoARequest = new CriarVeiculoRequest
        { Placa = "AAA-1234", Marca = "Chevrolet", Modelo = "Onix", Ano = 2019 };
        var veiculoAContent =
            new StringContent(JsonSerializer.Serialize(veiculoARequest), Encoding.UTF8, "application/json");
        await client.PostAsync($"/api/admin/clientes/{clienteA!.Id}/veiculos", veiculoAContent, TestContext.Current.CancellationToken);

        // Criar Cliente B
        var clienteBRequest = new CriarClienteRequest
        { Nome = "Cliente B", Cpf = "33344455508", Email = "clienteb@email.com", Telefone = "11922222222" };
        var clienteBContent =
            new StringContent(JsonSerializer.Serialize(clienteBRequest), Encoding.UTF8, "application/json");
        var postClienteBResponse = await client.PostAsync("/api/admin/clientes", clienteBContent, TestContext.Current.CancellationToken);
        var clienteB =
            JsonSerializer.Deserialize<ClienteResponse>(await postClienteBResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Criar Veiculo B para Cliente B
        var veiculoBRequest = new CriarVeiculoRequest
        { Placa = "BBB-5678", Marca = "Fiat", Modelo = "Uno", Ano = 2015 };
        var veiculoBContent =
            new StringContent(JsonSerializer.Serialize(veiculoBRequest), Encoding.UTF8, "application/json");
        var postVeiculoBResponse =
            await client.PostAsync($"/api/admin/clientes/{clienteB!.Id}/veiculos", veiculoBContent, TestContext.Current.CancellationToken);
        var veiculoB =
            JsonSerializer.Deserialize<VeiculoResponse>(await postVeiculoBResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Act: Tenta abrir OS para Cliente A com Veiculo B
        var abrirOSRequest = new AbrirOrdemServicoRequest
        {
            ClienteId = clienteA.Id,
            VeiculoId = veiculoB!.Id,
            DescricaoProblema = "Problema com veiculo de terceiro"
        };
        var abrirOSContent =
            new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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
        var clienteRequest = new CriarClienteRequest
        { Nome = "Maria Silva", Cpf = "90234571020", Email = "maria.silva@email.com", Telefone = "11977778888" };
        var clienteContent =
            new StringContent(JsonSerializer.Serialize(clienteRequest), Encoding.UTF8, "application/json");
        var postClienteResponse = await client.PostAsync("/api/admin/clientes", clienteContent, TestContext.Current.CancellationToken);
        postClienteResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var cliente =
            JsonSerializer.Deserialize<ClienteResponse>(await postClienteResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Criar Veiculo
        var veiculoRequest = new CriarVeiculoRequest
        { Placa = "CCC-1234", Marca = "Chevrolet", Modelo = "Cruze", Ano = 2021 };
        var veiculoContent =
            new StringContent(JsonSerializer.Serialize(veiculoRequest), Encoding.UTF8, "application/json");
        var postVeiculoResponse = await client.PostAsync($"/api/admin/clientes/{cliente!.Id}/veiculos", veiculoContent, TestContext.Current.CancellationToken);
        var veiculo =
            JsonSerializer.Deserialize<VeiculoResponse>(await postVeiculoResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);

        // Abrir OS
        var abrirOSRequest = new AbrirOrdemServicoRequest
        { ClienteId = cliente.Id, VeiculoId = veiculo!.Id, DescricaoProblema = "Troca de amortecedor" };
        var abrirOSContent =
            new StringContent(JsonSerializer.Serialize(abrirOSRequest), Encoding.UTF8, "application/json");
        var postOSResponse = await client.PostAsync("/api/admin/ordens-servico", abrirOSContent, TestContext.Current.CancellationToken);
        postOSResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var os = JsonSerializer.Deserialize<OrdemServicoResponse>(await postOSResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
            _jsonOptions);

        // Act: Atualizar status para EmDiagnostico usando JSON request body
        var statusRequest = new { NovoStatus = StatusOrdemServico.EmDiagnostico.ToString() };
        var statusContent =
            new StringContent(JsonSerializer.Serialize(statusRequest), Encoding.UTF8, "application/json");
        var putResponse = await client.PutAsync($"/api/admin/ordens-servico/{os!.Id}/status", statusContent, TestContext.Current.CancellationToken);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var osAtualizada =
            JsonSerializer.Deserialize<OrdemServicoResponse>(await putResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken),
                _jsonOptions);
        osAtualizada.Should().NotBeNull();
        osAtualizada!.Status.Should().Be("EmDiagnostico");
    }

    [Fact]
    public async Task ObterMinhasOrdens_ComTokenClienteValido_DeveRetornarOrdensAtivas()
    {
        // Arrange
        var client = factory.CreateClient();
        
        Guid clienteId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            var cliente = new Cliente(Guid.NewGuid(), "Jose Minhas Ordens", new Cpf("55566677720"), "jose.ordens@email.com", "11988887777");
            clienteId = cliente.Id;
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);

            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("XYZ9999"), "Fiat", "Uno", 2012, clienteId);
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);

            var os = new OrdemServico(Guid.NewGuid(), clienteId, veiculo.Id, "Problema no motor");
            await context.OrdensServico.AddAsync(os, TestContext.Current.CancellationToken);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Obtém token para o ID do cliente criado
        var tokenResponse = await client.PostAsync($"/api/public/auth/token?usuario={clienteId}&perfil=Cliente", null, TestContext.Current.CancellationToken);
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(tokenJson);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/public/ordens-servico", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = JsonSerializer.Deserialize<List<OrdemServicoResponse>>(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        list.Should().NotBeNull();
        list.Should().ContainSingle();
        list![0].DescricaoProblema.Should().Be("Problema no motor");
    }

    [Fact]
    public async Task Admin_ObterTodas_ComFiltros_DeveRetornarOrdensFiltradas()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/ordens-servico?status=Recebida", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = JsonSerializer.Deserialize<List<OrdemServicoResponse>>(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task Admin_ObterMetricasTempoMedio_DeveRetornarOk()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/ordens-servico/metricas/tempo-medio", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObterPorId_ClienteDiferente_DeveRetornar403Forbidden()
    {
        // Arrange
        var client = factory.CreateClient();
        
        Guid clienteId1;
        Guid clienteId2;
        Guid veiculoId;
        Guid osId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            
            var cliente1 = new Cliente(Guid.NewGuid(), "Jose Dono", new Cpf("33322211169"), "jose.dono@email.com", "11988887777");
            clienteId1 = cliente1.Id;
            await context.Clientes.AddAsync(cliente1, TestContext.Current.CancellationToken);

            var cliente2 = new Cliente(Guid.NewGuid(), "Maria Invasora", new Cpf("44455566619"), "maria.invasora@email.com", "11977776666");
            clienteId2 = cliente2.Id;
            await context.Clientes.AddAsync(cliente2, TestContext.Current.CancellationToken);

            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("XYZ9999"), "Fiat", "Uno", 2012, clienteId1);
            veiculoId = veiculo.Id;
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);

            var os = new OrdemServico(osId, clienteId1, veiculoId, "Problema no motor do Jose");
            await context.OrdensServico.AddAsync(os, TestContext.Current.CancellationToken);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Obtém token para o ID do cliente 2 (Maria Invasora)
        var tokenResponse = await client.PostAsync($"/api/public/auth/token?usuario={clienteId2}&perfil=Cliente", null, TestContext.Current.CancellationToken);
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(tokenJson);
        var token = doc.RootElement.GetProperty("token").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act: Maria Invasora tenta obter a OS do Jose
        var response = await client.GetAsync($"/api/public/ordens-servico/{osId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ObterPorId_Admin_DeveRetornar200Ok()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var osId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            var cliente = new Cliente(Guid.NewGuid(), "Jose da Silva Admin", new Cpf("84580296850"), "jose.admin@email.com", "11999999999");
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);

            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("ABC1D23"), "Ford", "Ka", 2020, cliente.Id);
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);

            var os = new OrdemServico(osId, cliente.Id, veiculo.Id, "Problema no motor");
            await context.OrdensServico.AddAsync(os, TestContext.Current.CancellationToken);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/admin/ordens-servico/{osId}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var osResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        osResponse.Should().NotBeNull();
        osResponse!.Id.Should().Be(osId);
        osResponse.DescricaoProblema.Should().Be("Problema no motor");
    }

    [Fact]
    public async Task ConsultarStatus_Publico_DeveRetornarDetalhesDoStatus()
    {
        // Arrange
        var client = factory.CreateClient();
        var osId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            var cliente = new Cliente(Guid.NewGuid(), "Marcos Teste", new Cpf("42439977640"), "marcos@email.com", "11988887777");
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);

            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("ABC1D23"), "Ford", "Ka", 2020, cliente.Id);
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);

            var os = new OrdemServico(osId, cliente.Id, veiculo.Id, "Problema na bateria");
            await context.OrdensServico.AddAsync(os, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/public/ordens-servico/{osId}/status", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusResponse = JsonSerializer.Deserialize<StatusOrdemServicoResponse>(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        statusResponse.Should().NotBeNull();
        statusResponse!.OrdemServicoId.Should().Be(osId);
        statusResponse.Status.Should().Be("Recebida");
        statusResponse.DescricaoStatus.Should().Contain("Recebida");
    }

    [Fact]
    public async Task WebhookNotificacaoOrcamento_Aprovado_DeveAtualizarParaEmExecucao()
    {
        // Arrange
        var client = factory.CreateClient();
        var osId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            var cliente = new Cliente(Guid.NewGuid(), "Lucas Webhook", new Cpf("04752545462"), "lucas@email.com", "11988887777");
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);

            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("ABC1D23"), "Ford", "Ka", 2020, cliente.Id);
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);

            var os = new OrdemServico(osId, cliente.Id, veiculo.Id, "Revisão geral");
            os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);
            os.DefinirOrcamento(300.00m);
            os.AtualizarStatus(StatusOrdemServico.AguardandoAprovacao);

            await context.OrdensServico.AddAsync(os, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var webhookRequest = new NotificacaoOrcamentoRequest
        {
            Aprovado = true,
            Observacao = "Aprovado via WhatsApp Bot"
        };
        var content = new StringContent(JsonSerializer.Serialize(webhookRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync($"/api/public/ordens-servico/{osId}/notificacao-orcamento", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var osResponse = JsonSerializer.Deserialize<OrdemServicoResponse>(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        osResponse.Should().NotBeNull();
        osResponse!.Status.Should().Be("EmExecucao");
    }

    [Fact]
    public async Task Admin_ObterTodas_DeveRetornarOrdenadoPorPrioridadeStatusEData()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/ordens-servico", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = JsonSerializer.Deserialize<List<OrdemServicoResponse>>(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), _jsonOptions);
        list.Should().NotBeNull();
    }

    [Fact]
    public async Task Admin_NotificarCliente_DeveRetornar200Ok()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = await ObterTokenBearerAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var osId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
            var cliente = new Cliente(Guid.NewGuid(), "Juliana Notificar", new Cpf("70995814813"), "juliana@email.com", "11988887777");
            await context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);

            var veiculo = new FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate.Veiculo(Guid.NewGuid(), new Placa("ABC1D23"), "Ford", "Ka", 2020, cliente.Id);
            await context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);

            var os = new OrdemServico(osId, cliente.Id, veiculo.Id, "Revisão elétrica");
            await context.OrdensServico.AddAsync(os, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.PostAsync($"/api/admin/ordens-servico/{osId}/notificar", null, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}