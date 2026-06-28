using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class AtualizarVeiculoUseCase(IVeiculoRepository veiculoRepository, IUnitOfWork unitOfWork)
{
    public async Task<VeiculoResponse> ExecutarAsync(Guid id, AtualizarVeiculoRequest request,
        CancellationToken cancellationToken = default)
    {
        var veiculo = await veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
            throw new DominioException("Veículo não encontrado.");

        var placa = new Placa(request.Placa);
        var existente = await veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (existente != null && existente.Id != id)
            throw new DominioException("Outro veículo já está cadastrado com esta placa.");

        veiculo.AlterarPlaca(placa);
        veiculo.AlterarMarca(request.Marca);
        veiculo.AlterarModelo(request.Modelo);
        veiculo.AlterarAno(request.Ano);

        await veiculoRepository.AtualizarAsync(veiculo, cancellationToken);
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
