using FIAP.Tech.Challenge.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace FIAP.Tech.Challenge.API.Controllers.Public;

/// <summary>
/// Perfil de usuário suportado pelo sistema de autorização (RBAC).
/// </summary>
public enum PerfilUsuario
{
    /// <summary>
    /// Perfil administrativo com acesso total a cadastros, estoque e faturamento.
    /// </summary>
    Admin,

    /// <summary>
    /// Perfil técnico com acesso a diagnósticos, catálogo de peças e status de OS.
    /// </summary>
    Mecanico,

    /// <summary>
    /// Perfil do cliente final para consulta e aprovação/rejeição de orçamentos.
    /// </summary>
    Cliente
}

/// <summary>
/// Controller de autenticação pública para geração de tokens JWT de teste.
/// </summary>
[ApiController]
[Route("api/public/auth")]
[Tags("Autenticacao")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController(TokenService tokenService) : ControllerBase
{
    /// <summary>
    /// ENDPOINT DE TESTE - Emite um token JWT temporário assinado localmente para validar o controle de acessos (RBAC) no Swagger.
    /// </summary>
    /// <remarks>
    /// Nota: Em cenários produtivos futuros, a autenticação e emissão de tokens serão delegadas a um Provedor de Identidade (IDP) federado, como o Keycloak.
    /// </remarks>
    /// <param name="usuario">Identificador fictício (ex: nome, CPF, ou Guid do cliente).</param>
    /// <param name="perfil">Perfil associado ao token (Admin, Mecanico, Cliente).</param>
    /// <response code="200">Token JWT gerado com sucesso.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpPost("token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult GerarTokenTeste([FromQuery] string usuario = "admin", [FromQuery] PerfilUsuario perfil = PerfilUsuario.Admin)
    {
        var token = tokenService.GerarToken(usuario, perfil.ToString());
        return Ok(new { token });
    }
}