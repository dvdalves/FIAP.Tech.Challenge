using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.Tech.Challenge.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class PecasController : ControllerBase
{
    private static readonly List<PecaDto> PecasEstoque = new()
    {
        new PecaDto { Id = Guid.NewGuid(), Nome = "Filtro de Óleo", Preco = 45.90m, QuantidadeEstoque = 15 },
        new PecaDto { Id = Guid.NewGuid(), Nome = "Pastilha de Freio", Preco = 180.00m, QuantidadeEstoque = 8 },
        new PecaDto { Id = Guid.NewGuid(), Nome = "Vela de Ignição", Preco = 25.50m, QuantidadeEstoque = 40 }
    };

    [HttpGet]
    public IActionResult ObterEstoque()
    {
        return Ok(PecasEstoque);
    }

    [HttpPost]
    public IActionResult AdicionarPeca([FromBody] PecaDto novaPeca)
    {
        if (string.IsNullOrWhiteSpace(novaPeca.Nome) || novaPeca.Preco <= 0)
            return BadRequest(new { erro = "Nome inválido ou preço menor/igual a zero." });

        novaPeca.Id = Guid.NewGuid();
        PecasEstoque.Add(novaPeca);
        return CreatedAtAction(nameof(ObterEstoque), new { id = novaPeca.Id }, novaPeca);
    }
}

public class PecaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
