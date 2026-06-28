using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Tech.Challenge.API.Controllers.Public;

[ApiController]
[Route("api/public/ordens-servico")]
public class OrdensServicoController(
    IOrdemServicoRepository ordemServicoRepository,
    AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
    RejeitarOrcamentoUseCase rejeitarOrcamentoUseCase)
    : ControllerBase
{
    [HttpGet]
    [Authorize]
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
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Cliente,Mecanico,Admin")]
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

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "Cliente")]
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

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = "Cliente")]
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