using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

[ExcludeFromCodeCoverage]
public class AtualizarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}
