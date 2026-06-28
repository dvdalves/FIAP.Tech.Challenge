using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
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
    [HttpPost]
    public async Task<IActionResult> Abrir([FromBody] AbrirOrdemServicoRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await abrirOrdemServicoUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtRoute(new { controller = "OrdensServico", id = osResponse.Id }, osResponse);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatus(
        Guid id,
        [FromBody] AtualizarStatusRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await atualizarStatusOsUseCase.ExecutarAsync(id, request.NovoStatus, cancellationToken);
        return Ok(osResponse);
    }

    [HttpPost("{id:guid}/itens")]
    public async Task<IActionResult> LancarItens(
        Guid id,
        [FromBody] LancarItensOSRequest request,
        CancellationToken cancellationToken)
    {
        var osResponse = await lancarItensOsUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(osResponse);
    }
}