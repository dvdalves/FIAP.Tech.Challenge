using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.API.Controllers.Public;

[ApiController]
[Route("api/public/ordens-servico")]
public class OrdensServicoController(
    IOrdemServicoRepository ordemServicoRepository,
    AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
    RejeitarOrcamentoUseCase rejeitarOrcamentoUseCase)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

        return Ok(os.ParaResponse());
    }

    [HttpPost("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id, CancellationToken cancellationToken)
    {
        var osResponse = await aprovarOrcamentoUseCase.ExecutarAsync(id, cancellationToken);
        return Ok(osResponse);
    }

    [HttpPost("{id:guid}/rejeitar")]
    public async Task<IActionResult> Rejeitar(Guid id, CancellationToken cancellationToken)
    {
        var osResponse = await rejeitarOrcamentoUseCase.ExecutarAsync(id, cancellationToken);
        return Ok(osResponse);
    }
}
