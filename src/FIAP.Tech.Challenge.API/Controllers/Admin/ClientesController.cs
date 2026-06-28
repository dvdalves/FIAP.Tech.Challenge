using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

/// <summary>
/// Controller administrativo para gestão de clientes e seus veículos (frotas).
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/clientes")]
[Tags("Clientes")]
[Produces(MediaTypeNames.Application.Json)]
public class ClientesController(
    IClienteRepository clienteRepository,
    CriarClienteUseCase criarClienteUseCase,
    CriarVeiculoUseCase criarVeiculoUseCase)
    : ControllerBase
{
    /// <summary>
    /// Lista todos os clientes cadastrados com suas frotas de veículos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de clientes retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Cliente>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var clientes = await clienteRepository.ObterTodosAsync(cancellationToken);
        return Ok(clientes);
    }

    /// <summary>
    /// Obtém os detalhes de um cliente específico pelo seu ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Cliente retornado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Cliente não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Cliente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado." });

        return Ok(cliente);
    }

    /// <summary>
    /// Cadastra um novo cliente no sistema.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/admin/clientes
    ///     {
    ///        "nome": "João da Silva",
    ///        "cpf": "12345678909",
    ///        "email": "joao@email.com",
    ///        "telefone": "11988887777"
    ///     }
    /// </remarks>
    /// <param name="request">Dados do cliente para cadastro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="201">Cliente criado com sucesso.</response>
    /// <response code="400">Dados inválidos ou cliente com mesmo CPF já cadastrado.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] CriarClienteRequest request, CancellationToken cancellationToken)
    {
        var response = await criarClienteUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    /// <summary>
    /// Vincula um veículo à frota de um cliente existente.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/admin/clientes/3fa85f64-5717-4562-b3fc-2c963f66afa6/veiculos
    ///     {
    ///        "placa": "ABC1D23",
    ///        "marca": "Ford",
    ///        "modelo": "Focus",
    ///        "ano": 2018
    ///     }
    /// </remarks>
    /// <param name="id">ID do cliente proprietário.</param>
    /// <param name="request">Dados do veículo para cadastro.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Veículo vinculado com sucesso.</response>
    /// <response code="400">Dados inválidos (ex: placa no formato incorreto).</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (requer perfil Admin).</response>
    /// <response code="404">Cliente não encontrado.</response>
    [HttpPost("{id:guid}/veiculos")]
    [ProducesResponseType(typeof(VeiculoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CriarVeiculo(Guid id, [FromBody] CriarVeiculoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await criarVeiculoUseCase.ExecutarAsync(id, request, cancellationToken);
        return Ok(response);
    }
}