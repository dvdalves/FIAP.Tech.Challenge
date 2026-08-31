using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class NotificacaoOrcamentoRequest
{
    public bool Aprovado { get; set; }
    public string? Observacao { get; set; }
}
