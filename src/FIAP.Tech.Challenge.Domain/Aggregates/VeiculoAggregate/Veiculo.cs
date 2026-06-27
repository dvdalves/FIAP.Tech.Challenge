using System;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;

public class Veiculo
{
    public Guid Id { get; private set; }
    public Placa Placa { get; private set; } = null!;
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int Ano { get; private set; }
    public Guid ClienteId { get; private set; }

    // EF Core constructor
    private Veiculo() { }

    public Veiculo(Guid id, Placa placa, string marca, string modelo, int ano, Guid clienteId)
    {
        if (id == Guid.Empty)
            throw new DominioException("Id do veículo inválido.");
        if (clienteId == Guid.Empty)
            throw new DominioException("Id do cliente associado ao veículo é obrigatório.");
        
        AlterarPlaca(placa);
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarAno(ano);

        Id = id;
        ClienteId = clienteId;
    }

    public void AlterarPlaca(Placa placa)
    {
        Placa = placa ?? throw new DominioException("Placa do veículo é obrigatória.");
    }

    public void AlterarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new DominioException("Marca do veículo é obrigatória.");
        Marca = marca.Trim();
    }

    public void AlterarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new DominioException("Modelo do veículo é obrigatório.");
        Modelo = modelo.Trim();
    }

    public void AlterarAno(int ano)
    {
        int anoAtual = DateTime.Now.Year;
        if (ano < 1886 || ano > anoAtual + 2)
            throw new DominioException($"Ano do veículo deve estar entre 1886 e {anoAtual + 2}.");
        Ano = ano;
    }
}
