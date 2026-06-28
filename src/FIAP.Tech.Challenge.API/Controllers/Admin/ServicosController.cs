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
    CriarServicoUseCase criarServicoUseCase,
    AtualizarServicoUseCase atualizarServicoUseCase,
    ExcluirServicoUseCase excluirServicoUseCase)
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
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    /// <summary>
    /// Obtém os detalhes de um serviço específico pelo seu ID.
    /// </summary>
    /// <param name="id">ID do serviço.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Serviço retornado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin ou Mecanico).</response>
    /// <response code="404">Serviço não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Mecanico,Admin")]
    [ProducesResponseType(typeof(Servico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var servico = await servicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (servico == null)
            return NotFound(new { mensagem = "Serviço não encontrado." });

        return Ok(servico);
    }

    /// <summary>
    /// Atualiza os dados de um serviço no catálogo.
    /// </summary>
    /// <param name="id">ID do serviço.</param>
    /// <param name="request">Novos dados do serviço.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Serviço atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos (ex: preço menor que zero).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Serviço não encontrado.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarServicoRequest request, CancellationToken cancellationToken)
    {
        var response = await atualizarServicoUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Exclui um serviço do catálogo, se não estiver associado a nenhuma ordem de serviço.
    /// </summary>
    /// <param name="id">ID do serviço.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="204">Serviço excluído com sucesso.</response>
    /// <response code="400">Não é possível excluir devido a vínculos ativos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Serviço não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await excluirServicoUseCase.ExecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
