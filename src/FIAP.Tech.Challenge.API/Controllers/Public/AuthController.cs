using Microsoft.AspNetCore.Mvc;
using FIAP.Tech.Challenge.Infrastructure.Services;

namespace FIAP.Tech.Challenge.API.Controllers.Public;

[ApiController]
[Route("api/public/auth")]
public class AuthController(TokenService tokenService) : ControllerBase
{
    /// <summary>
    /// ENDPOINT DE TESTE - USO EXCLUSIVO PARA VALIDAÇÃO DO MVP DO PRODUTO.
    /// Gera um token JWT próprio assinado localmente de forma temporária para testar as APIs administrativas.
    /// Em produção futura, a emissão e gerenciamento de tokens deve ser feito pelo Keycloak.
    /// </summary>
    /// <param name="usuario">Identificador fictício do usuário.</param>
    /// <param name="perfil">Perfil associado ao token (ex: Admin).</param>
    /// <returns>Token JWT gerado.</returns>
    [HttpPost("token")]
    public IActionResult GerarTokenTeste([FromQuery] string usuario = "admin", [FromQuery] string perfil = "Admin")
    {
        var token = tokenService.GerarToken(usuario, perfil);
        return Ok(new { token });
    }
}
