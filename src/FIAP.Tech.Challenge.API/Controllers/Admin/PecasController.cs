using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/pecas")]
public class PecasController(
    IPecaRepository pecaRepository,
    AjustarEstoquePecaUseCase ajustarEstoquePecaUseCase)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Mecanico,Admin")]
    public async Task<IActionResult> ObterEstoque(CancellationToken cancellationToken)
    {
        var pecas = await pecaRepository.ObterTodasAsync(cancellationToken);
        var dtos = pecas.Select(p => new PecaResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            Preco = p.Preco,
            QuantidadeEstoque = p.QuantidadeEstoque
        });
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdicionarPeca([FromBody] AdicionarPecaRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || request.Preco <= 0)
            return BadRequest(new { erro = "Nome inválido ou preço menor/igual a zero." });

        var peca = new Peca(
            Guid.NewGuid(),
            request.Nome,
            request.Preco,
            request.QuantidadeEstoque
        );

        await pecaRepository.AdicionarAsync(peca, cancellationToken);
        return CreatedAtAction(nameof(ObterEstoque), new { id = peca.Id }, new PecaResponse
        {
            Id = peca.Id,
            Nome = peca.Nome,
            Preco = peca.Preco,
            QuantidadeEstoque = peca.QuantidadeEstoque
        });
    }

    [HttpPut("{id:guid}/estoque")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AjustarEstoque(Guid id, [FromQuery] int quantidade,
        CancellationToken cancellationToken)
    {
        await ajustarEstoquePecaUseCase.ExecutarAsync(id, quantidade, cancellationToken);
        return Ok(new { mensagem = "Estoque atualizado com sucesso." });
    }
}