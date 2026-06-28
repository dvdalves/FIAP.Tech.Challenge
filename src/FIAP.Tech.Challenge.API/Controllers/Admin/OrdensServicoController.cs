using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

/// <summary>
/// Controller administrativo para controle e execução do fluxo de Ordens de Serviço (OS).
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/ordens-servico")]
[Tags("OrdensServico")]
[Produces(MediaTypeNames.Application.Json)]
public class OrdensServicoController(
    AbrirOrdemServicoUseCase abrirOrdemServicoUseCase,
    AtualizarStatusOSUseCase atualizarStatusOsUseCase,
    LancarItensOSUseCase lancarItensOsUseCase)
    : ControllerBase
{
    /// <summary>
    /// Lista todas as Ordens de Serviço cadastradas no sistema, com suporte a filtros.
    /// </summary>
    /// <param name="status">Filtrar por status da OS (opcional).</param>
    /// <param name="clienteId">Filtrar por ID do cliente (opcional).</param>
    /// <param name="repository">Repositório de ordens de serviço.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de Ordens de Serviço retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin ou Mecanico).</response>
    [HttpGet]
    [Authorize(Roles = "Mecanico,Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterTodas(
        [FromQuery] StatusOrdemServico? status,
        [FromQuery] Guid? clienteId,
        [FromServices] IOrdemServicoRepository repository,
        CancellationToken cancellationToken)
    {
        var ordens = await repository.ObterTodasAsync(cancellationToken);

        if (status.HasValue)
            ordens = ordens.Where(o => o.Status == status.Value);

        if (clienteId.HasValue)
            ordens = ordens.Where(o => o.ClienteId == clienteId.Value);

        return Ok(ordens.Select(o => o.ParaResponse()));
    }

    /// <summary>
    /// Obtém a métrica de tempo médio de execução das Ordens de Serviço finalizadas.
    /// </summary>
    /// <param name="repository">Repositório de ordens de serviço.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Métricas calculadas com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpGet("metricas/tempo-medio")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterTempoMedioExecucao(
        [FromServices] IOrdemServicoRepository repository,
        CancellationToken cancellationToken)
    {
        var ordens = await repository.ObterTodasAsync(cancellationToken);

        var ordensFinalizadas = ordens.Where(o =>
            o.Status == StatusOrdemServico.Finalizada &&
            o.DataInicioExecucao.HasValue &&
            o.DataFinalizacao.HasValue);

        if (!ordensFinalizadas.Any())
        {
            return Ok(new
            {
                mensagem = "Nenhuma ordem de serviço finalizada com tempos registrados para calcular a média.",
                tempoMedioHoras = 0,
                totalOrdensFinalizadas = 0
            });
        }

        double totalHoras = 0;
        foreach (var os in ordensFinalizadas)
        {
            var diff = os.DataFinalizacao!.Value - os.DataInicioExecucao!.Value;
            totalHoras += diff.TotalHours;
        }

        var tempoMedioHoras = totalHoras / ordensFinalizadas.Count();

        return Ok(new
        {
            tempoMedioHoras = Math.Round(tempoMedioHoras, 2),
            totalOrdensFinalizadas = ordensFinalizadas.Count()
        });
    }

    /// <summary>
    /// Abre uma nova Ordem de Serviço na recepção (status inicial: Recebida).
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/admin/ordens-servico
    ///     {
    ///        "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
    ///        "descricaoProblema": "Troca de óleo e barulho na suspensão dianteira."
    ///     }
    /// </remarks>
    /// <param name="request">Dados necessários para a abertura da OS.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="201">Ordem de Serviço criada com sucesso.</response>
    /// <response code="400">Dados inválidos ou inconsistência de cliente/veículo.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Abrir([FromBody] AbrirOrdemServicoRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await abrirOrdemServicoUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtRoute(new { controller = "OrdensServico", id = osResponse.Id }, osResponse);
    }

    /// <summary>
    /// Atualiza manualmente o status de uma Ordem de Serviço.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     PUT /api/admin/ordens-servico/3fa85f64-5717-4562-b3fc-2c963f66afa6/status
    ///     {
    ///        "novoStatus": 4
    ///     }
    ///     
    /// Onde o novoStatus pode ser:
    /// - 0 = Recebida
    /// - 1 = EmDiagnostico
    /// - 2 = AguardandoAprovacao
    /// - 3 = EmExecucao
    /// - 4 = Finalizada
    /// - 5 = Entregue
    /// - 6 = Cancelada
    /// </remarks>
    /// <param name="id">ID da Ordem de Serviço.</param>
    /// <param name="request">Payload contendo o novo status desejado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Status atualizado com sucesso e retorno da OS atualizada.</response>
    /// <response code="400">Transição de status inválida conforme as regras do fluxo de negócio.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (Mecânicos não podem alterar para o status "Entregue").</response>
    /// <response code="404">Ordem de Serviço não encontrada.</response>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Mecanico,Admin")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(
        Guid id,
        [FromBody] AtualizarStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.NovoStatus == StatusOrdemServico.Entregue && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var osResponse = await atualizarStatusOsUseCase.ExecutarAsync(id, request.NovoStatus, cancellationToken);
        return Ok(osResponse);
    }

    /// <summary>
    /// Lança as peças e serviços necessários após a avaliação técnica (diagnóstico) do veículo.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/admin/ordens-servico/3fa85f64-5717-4562-b3fc-2c963f66afa6/itens
    ///     {
    ///        "itensPeca": [
    ///           { "pecaId": "5ca85f64-5717-4562-b3fc-2c963f66afc4", "quantidade": 2 }
    ///        ],
    ///        "itensServico": [
    ///           { "servicoId": "6ca85f64-5717-4562-b3fc-2c963f66afd5" }
    ///        ]
    ///     }
    /// </remarks>
    /// <param name="id">ID da Ordem de Serviço.</param>
    /// <param name="request">Lista de peças e serviços orçados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Itens lançados com sucesso e orçamento gerado.</response>
    /// <response code="400">Dados inválidos (ex: peças ou serviços inexistentes no catálogo).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin ou Mecanico).</response>
    /// <response code="404">Ordem de Serviço não encontrada.</response>
    [HttpPost("{id:guid}/itens")]
    [Authorize(Roles = "Mecanico,Admin")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LancarItens(
        Guid id,
        [FromBody] LancarItensOSRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await lancarItensOsUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(osResponse);
    }
}