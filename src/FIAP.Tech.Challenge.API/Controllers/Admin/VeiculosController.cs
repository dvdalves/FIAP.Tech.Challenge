using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

/// <summary>
/// Controller administrativo para gestão de veículos.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/veiculos")]
[Tags("Veiculos")]
[Produces(MediaTypeNames.Application.Json)]
public class VeiculosController(
    IVeiculoRepository veiculoRepository,
    AtualizarVeiculoUseCase atualizarVeiculoUseCase,
    ExcluirVeiculoUseCase excluirVeiculoUseCase)
    : ControllerBase
{
    /// <summary>
    /// Lista todos os veículos cadastrados no sistema (suporta filtro por cliente).
    /// </summary>
    /// <param name="clienteId">ID do cliente proprietário (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de veículos retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Veiculo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterTodos([FromQuery] Guid? clienteId, CancellationToken cancellationToken)
    {
        if (clienteId.HasValue)
        {
            var veiculosCliente = await veiculoRepository.ObterPorClienteIdAsync(clienteId.Value, cancellationToken);
            return Ok(veiculosCliente);
        }

        // Se não tiver repositório com "ObterTodos", podemos usar o contexto ou adicionar na interface do repositório,
        // mas espere, IVeiculoRepository não tem ObterTodos!
        // Vamos ver: IVeiculoRepository has:
        // Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        // Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken cancellationToken = default);
        // Task<IEnumerable<Veiculo>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
        // Wait, if it doesn't have ObterTodosAsync, let's add ObterTodosAsync to IVeiculoRepository and implement it in VeiculoRepository!
        // That is extremely easy. Or we can just get all via context, but Repository is better for Domain agnostic design.
        // Let's add ObterTodosAsync to IVeiculoRepository.
        var veiculos = await veiculoRepository.ObterTodosAsync(cancellationToken);
        return Ok(veiculos);
    }

    /// <summary>
    /// Obtém os detalhes de um veículo específico pelo seu ID.
    /// </summary>
    /// <param name="id">ID do veículo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Veículo retornado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Veículo não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Veiculo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var veiculo = await veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
            return NotFound(new { mensagem = "Veículo não encontrado." });

        return Ok(veiculo);
    }

    /// <summary>
    /// Atualiza os dados de um veículo cadastrado.
    /// </summary>
    /// <param name="id">ID do veículo.</param>
    /// <param name="request">Novos dados do veículo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Veículo atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos ou placa em formato incorreto.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Veículo não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVeiculoRequest request, CancellationToken cancellationToken)
    {
        var response = await atualizarVeiculoUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Exclui um veículo do cadastro, se não possuir ordens de serviço vinculadas.
    /// </summary>
    /// <param name="id">ID do veículo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="204">Veículo excluído com sucesso.</response>
    /// <response code="400">Não é possível excluir devido a vínculos ativos.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Veículo não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await excluirVeiculoUseCase.ExecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
