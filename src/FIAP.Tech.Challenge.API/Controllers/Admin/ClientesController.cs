using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;

    public ClientesController(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.ObterTodosAsync(cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
            return NotFound(new { mensagem = "Cliente não encontrado." });

        return Ok(cliente);
    }
}
