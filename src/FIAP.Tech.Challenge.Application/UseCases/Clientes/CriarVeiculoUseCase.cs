using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class CriarVeiculoUseCase(
    IVeiculoRepository veiculoRepository,
    IClienteRepository clienteRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<VeiculoResponse> ExecutarAsync(Guid clienteId, CriarVeiculoRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar se o cliente associado existe
        var cliente = await clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);
        if (cliente == null)
            throw new DominioException("Cliente não encontrado.");

        var placa = new Placa(request.Placa);

        // 2. Verificar se o veículo já está registrado com essa placa
        var existente = await veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (existente != null)
        {
            if (existente.ClienteId == clienteId)
                throw new DominioException("Este veículo já está cadastrado para este cliente.");

            throw new DominioException("Este veículo já está cadastrado para outro cliente.");
        }

        var veiculo = new Veiculo(
            Guid.NewGuid(),
            placa,
            request.Marca,
            request.Modelo,
            request.Ano,
            clienteId
        );

        await veiculoRepository.AdicionarAsync(veiculo, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new VeiculoResponse
        {
            Id = veiculo.Id,
            Placa = veiculo.Placa.Valor,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano,
            ClienteId = veiculo.ClienteId
        };
    }
}