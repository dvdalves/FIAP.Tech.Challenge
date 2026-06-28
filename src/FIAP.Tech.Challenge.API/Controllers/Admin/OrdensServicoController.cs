using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/ordens-servico")]
public class OrdensServicoController(
    AbrirOrdemServicoUseCase abrirOrdemServicoUseCase,
    AtualizarStatusOSUseCase atualizarStatusOsUseCase,
    LancarItensOSUseCase lancarItensOsUseCase)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Mecanico,Admin")]
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

    [HttpGet("metricas/tempo-medio")]
    [Authorize(Roles = "Admin")]
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Abrir([FromBody] AbrirOrdemServicoRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await abrirOrdemServicoUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtRoute(new { controller = "OrdensServico", id = osResponse.Id }, osResponse);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Mecanico,Admin")]
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

    [HttpPost("{id:guid}/itens")]
    [Authorize(Roles = "Mecanico,Admin")]
    public async Task<IActionResult> LancarItens(
        Guid id,
        [FromBody] LancarItensOSRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await lancarItensOsUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(osResponse);
    }
}