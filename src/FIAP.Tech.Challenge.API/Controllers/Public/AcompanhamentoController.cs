using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Infrastructure.Services;

namespace FIAP.Tech.Challenge.API.Controllers.Public;

[ApiController]
[Route("api/public/[controller]")]
public class AcompanhamentoController : ControllerBase
{
    private readonly CriarOrdemServicoUseCase _criarOrdemServicoUseCase;
    private readonly AtualizarStatusOSUseCase _atualizarStatusOSUseCase;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly TokenService _tokenService;

    public AcompanhamentoController(
        CriarOrdemServicoUseCase criarOrdemServicoUseCase,
        AtualizarStatusOSUseCase atualizarStatusOSUseCase,
        IOrdemServicoRepository ordemServicoRepository,
        TokenService tokenService)
    {
        _criarOrdemServicoUseCase = criarOrdemServicoUseCase;
        _atualizarStatusOSUseCase = atualizarStatusOSUseCase;
        _ordemServicoRepository = ordemServicoRepository;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarOrdemServicoRequest request, CancellationToken cancellationToken)
    {
        var osResponse = await _criarOrdemServicoUseCase.ExecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = osResponse.Id }, osResponse);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var os = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            return NotFound(new { mensagem = "Ordem de serviço não encontrada." });

        return Ok(os.ParaResponse());
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatus(
        Guid id, 
        [FromQuery] StatusOrdemServico novoStatus, 
        [FromQuery] decimal? valorOrcamento, 
        CancellationToken cancellationToken)
    {
        var osResponse = await _atualizarStatusOSUseCase.ExecutarAsync(id, novoStatus, valorOrcamento, cancellationToken);
        return Ok(osResponse);
    }

    // Endpoint público para gerar token JWT de teste
    [HttpPost("token")]
    public IActionResult GerarTokenTeste([FromQuery] string usuario = "admin", [FromQuery] string perfil = "Admin")
    {
        var token = _tokenService.GerarToken(usuario, perfil);
        return Ok(new { token });
    }
}
