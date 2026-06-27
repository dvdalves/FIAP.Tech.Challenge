namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class VeiculoResponse
{
    public Guid Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public Guid ClienteId { get; set; }
}