using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Public;

/// <summary>
/// Controller público para consulta e aprovação/rejeição de orçamentos pelo cliente final.
/// </summary>
[ApiController]
[Route("api/public/ordens-servico")]
[Tags("OrdensServico")]
[Produces(MediaTypeNames.Application.Json)]
public class OrdensServicoController(
    IOrdemServicoRepository ordemServicoRepository,
    AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
    RejeitarOrcamentoUseCase rejeitarOrcamentoUseCase)
    : ControllerBase
{
    /// <summary>
    /// Lista as Ordens de Serviço ativas (não concluídas ou canceladas) vinculadas ao cliente autenticado.
    /// </summary>
    /// <param name="clienteRepository">Repositório de clientes para resolução de CPF.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de Ordens de Serviço ativas do cliente retornada com sucesso.</response>
    /// <response code="400">Identificação do cliente inválida ou não encontrada no token JWT.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="404">Cliente correspondente ao token não cadastrado no sistema.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterMinhasOrdens(
        [FromServices] IClienteRepository clienteRepository,
        CancellationToken cancellationToken)
    {
        var claimName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(claimName))
            return BadRequest(new { mensagem = "Identificação do cliente não encontrada no token." });

        Guid? clienteId = null;

        if (Guid.TryParse(claimName, out var idParsed))
        {
            clienteId = idParsed;
        }
        else
        {
            try
            {
                var cliente = await clienteRepository.ObterPorCpfAsync(new FIAP.Tech.Challenge.Domain.ValueObjects.Cpf(claimName), cancellationToken);
                if (cliente != null)
                {
                    clienteId = cliente.Id;
                }
            }
            catch
            {
                // CPF inválido ou erro na busca
            }
        }

        if (!clienteId.HasValue)
            return NotFound(new { mensagem = "Cliente correspondente ao token não encontrado." });

        var todasOs = await ordemServicoRepository.ObterTodasAsync(cancellationToken);

        var osAtivas = todasOs.Where(o =>
            o.ClienteId == clienteId.Value &&
            o.Status != StatusOrdemServico.Finalizada &&
            o.Status != StatusOrdemServico.Entregue &&
            o.Status != StatusOrdemServico.Cancelada);

        return Ok(osAtivas.Select(o => o.ParaResponse()));
    }

    /// <summary>
    /// Obtém detalhes de uma Ordem de Serviço específica.
    /// </summary>
    /// <param name="id">ID da Ordem de Serviço.</param>
    /// <param name="clienteRepository">Repositório de clientes.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Ordem de Serviço retornada com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (Clientes não podem visualizar ordens de terceiros).</response>
    /// <response code="404">Ordem de Serviço não encontrada.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Cliente,Mecanico,Admin")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, [FromServices] IClienteRepository clienteRepository, CancellationToken cancellationToken)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

        // Validação de segurança: se for cliente, só pode visualizar se a OS for dele
        var claimName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var isCliente = User.IsInRole("Cliente");
        if (isCliente && !string.IsNullOrEmpty(claimName))
        {
            Guid? clienteId = null;
            if (Guid.TryParse(claimName, out var idParsed))
            {
                clienteId = idParsed;
            }
            else
            {
                try
                {
                    var cliente = await clienteRepository.ObterPorCpfAsync(new FIAP.Tech.Challenge.Domain.ValueObjects.Cpf(claimName), cancellationToken);
                    if (cliente != null) clienteId = cliente.Id;
                }
                catch
                {
                    // erro na busca
                }
            }

            if (os.ClienteId != clienteId)
            {
                return Forbid();
            }
        }

        return Ok(os.ParaResponse());
    }

    /// <summary>
    /// Aprova o orçamento de uma Ordem de Serviço, autorizando o início da execução física dos reparos e baixando o estoque das peças.
    /// </summary>
    /// <remarks>
    /// Apenas o próprio cliente proprietário do veículo pode aprovar a OS. O status transiciona para "EmExecucao".
    /// </remarks>
    /// <param name="id">ID da Ordem de Serviço.</param>
    /// <param name="clienteRepository">Repositório de clientes.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Orçamento aprovado com sucesso e OS atualizada.</response>
    /// <response code="400">Transição inválida ou estoque insuficiente para as peças reservadas.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (apenas o cliente proprietário pode aprovar).</response>
    /// <response code="404">Ordem de Serviço não encontrada.</response>
    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "Cliente")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprovar(Guid id, [FromServices] IClienteRepository clienteRepository, CancellationToken cancellationToken)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

        var claimName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(claimName))
            return BadRequest(new { mensagem = "Identificação do cliente não encontrada no token." });

        Guid? clienteId = null;
        if (Guid.TryParse(claimName, out var idParsed))
        {
            clienteId = idParsed;
        }
        else
        {
            try
            {
                var cliente = await clienteRepository.ObterPorCpfAsync(new FIAP.Tech.Challenge.Domain.ValueObjects.Cpf(claimName), cancellationToken);
                if (cliente != null) clienteId = cliente.Id;
            }
            catch
            {
                // erro na busca
            }
        }

        if (os.ClienteId != clienteId)
        {
            return Forbid();
        }

        var osResponse = await aprovarOrcamentoUseCase.ExecutarAsync(id, cancellationToken);
        return Ok(osResponse);
    }

    /// <summary>
    /// Rejeita o orçamento de uma Ordem de Serviço, cancelando os reparos.
    /// </summary>
    /// <remarks>
    /// Apenas o próprio cliente proprietário do veículo pode rejeitar a OS. O status transiciona para "Cancelada".
    /// </remarks>
    /// <param name="id">ID da Ordem de Serviço.</param>
    /// <param name="clienteRepository">Repositório de clientes.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Orçamento rejeitado com sucesso e OS cancelada.</response>
    /// <response code="400">Transição inválida de status.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Acesso negado (apenas o cliente proprietário pode rejeitar).</response>
    /// <response code="404">Ordem de Serviço não encontrada.</response>
    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = "Cliente")]
    [ProducesResponseType(typeof(OrdemServicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rejeitar(Guid id, [FromServices] IClienteRepository clienteRepository, CancellationToken cancellationToken)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

        var claimName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(claimName))
            return BadRequest(new { mensagem = "Identificação do cliente não encontrada no token." });

        Guid? clienteId = null;
        if (Guid.TryParse(claimName, out var idParsed))
        {
            clienteId = idParsed;
        }
        else
        {
            try
            {
                var cliente = await clienteRepository.ObterPorCpfAsync(new FIAP.Tech.Challenge.Domain.ValueObjects.Cpf(claimName), cancellationToken);
                if (cliente != null) clienteId = cliente.Id;
            }
            catch
            {
                // erro na busca
            }
        }

        if (os.ClienteId != clienteId)
        {
            return Forbid();
        }

        var osResponse = await rejeitarOrcamentoUseCase.ExecutarAsync(id, cancellationToken);
        return Ok(osResponse);
    }
}