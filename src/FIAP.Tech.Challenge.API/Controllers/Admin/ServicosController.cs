using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

/// <summary>
/// Controller administrativo para gestão do catálogo de tipos de serviços prestados pela oficina.
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/servicos")]
[Tags("Servicos")]
[Produces(MediaTypeNames.Application.Json)]
public class ServicosController(
    IServicoRepository servicoRepository,
    CriarServicoUseCase criarServicoUseCase)
    : ControllerBase
{
    /// <summary>
    /// Obtém todos os serviços de mão de obra cadastrados no catálogo.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de serviços retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin ou Mecanico).</response>
    [HttpGet]
    [Authorize(Roles = "Mecanico,Admin")]
    [ProducesResponseType(typeof(IEnumerable<Servico>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var servicos = await servicoRepository.ObterTodosAsync(cancellationToken);
        return Ok(servicos);
    }

    /// <summary>
    /// Cadastra um novo serviço no catálogo.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/admin/servicos
    ///     {
    ///        "descricao": "Alinhamento e Balanceamento",
    ///        "precoMaoDeObra": 120.00
    ///     }
    /// </remarks>
    /// <param name="request">Dados do serviço a ser cadastrado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="201">Serviço cadastrado com sucesso.</response>
    /// <response code="400">Dados inválidos (ex: valor de mão de obra menor ou igual a zero).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cadastrar([FromBody] CriarServicoRequest request, CancellationToken cancellationToken)
    {
        var response = await criarServicoUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterTodos), new { id = response.Id }, response);
    }
}
