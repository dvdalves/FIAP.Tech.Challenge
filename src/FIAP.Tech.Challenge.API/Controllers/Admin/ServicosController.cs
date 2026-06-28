using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/servicos")]
public class ServicosController(
    IServicoRepository servicoRepository,
    CriarServicoUseCase criarServicoUseCase)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Mecanico,Admin")]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var servicos = await servicoRepository.ObterTodosAsync(cancellationToken);
        return Ok(servicos);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cadastrar([FromBody] CriarServicoRequest request, CancellationToken cancellationToken)
    {
        var response = await criarServicoUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterTodos), new { id = response.Id }, response);
    }
}
