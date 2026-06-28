using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

/// <summary>
/// Controller administrativo para gestão do catálogo de peças e controle de saldo em estoque.
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/pecas")]
[Tags("Pecas")]
[Produces(MediaTypeNames.Application.Json)]
public class PecasController(
    IPecaRepository pecaRepository,
    AjustarEstoquePecaUseCase ajustarEstoquePecaUseCase)
    : ControllerBase
{
    /// <summary>
    /// Obtém o saldo em estoque de todas as peças cadastradas no catálogo.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de peças e saldos retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin ou Mecanico).</response>
    [HttpGet]
    [Authorize(Roles = "Mecanico,Admin")]
    [ProducesResponseType(typeof(IEnumerable<PecaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Cadastra uma nova peça no catálogo da oficina.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/admin/pecas
    ///     {
    ///        "nome": "Pastilha de Freio Dianteira",
    ///        "preco": 189.90,
    ///        "quantidadeEstoque": 15
    ///     }
    /// </remarks>
    /// <param name="request">Dados da peça a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="201">Peça cadastrada com sucesso.</response>
    /// <response code="400">Nome inválido ou preço menor/igual a zero.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PecaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// Ajusta manualmente o saldo físico em estoque de uma peça específica.
    /// </summary>
    /// <param name="id">ID da peça no catálogo.</param>
    /// <param name="quantidade">Nova quantidade em estoque.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Saldo atualizado com sucesso.</response>
    /// <response code="400">Quantidade inválida (menor que zero).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Peça não encontrada no catálogo.</response>
    [HttpPut("{id:guid}/estoque")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AjustarEstoque(Guid id, [FromQuery] int quantidade,
        CancellationToken cancellationToken)
    {
        await ajustarEstoquePecaUseCase.ExecutarAsync(id, quantidade, cancellationToken);
        return Ok(new { mensagem = "Estoque atualizado com sucesso." });
    }
}