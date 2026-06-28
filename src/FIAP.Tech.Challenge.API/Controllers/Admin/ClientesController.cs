using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/clientes")]
public class ClientesController(
    IClienteRepository clienteRepository,
    CriarClienteUseCase criarClienteUseCase,
    CriarVeiculoUseCase criarVeiculoUseCase)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var clientes = await clienteRepository.ObterTodosAsync(cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado." });

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarClienteRequest request, CancellationToken cancellationToken)
    {
        var response = await criarClienteUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpPost("{id:guid}/veiculos")]
    public async Task<IActionResult> CriarVeiculo(Guid id, [FromBody] CriarVeiculoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await criarVeiculoUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(response);
    }
}