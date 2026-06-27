using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AbrirOrdemServicoRequest
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
}

public class AbrirOrdemServicoUseCase(
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(AbrirOrdemServicoRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Validar se o cliente existe
        var cliente = await clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente == null)
            throw new DominioException("Cliente não encontrado.");

        // 2. Validar se o veículo existe
        var veiculo = await veiculoRepository.ObterPorIdAsync(request.VeiculoId, cancellationToken);
        if (veiculo == null)
            throw new DominioException("Veículo não encontrado.");

        // 3. Validar se o veículo pertence a este cliente
        if (veiculo.ClienteId != cliente.Id)
            throw new DominioException("Este veículo não pertence ao cliente informado.");

        // 4. Criar OS
        var os = new OrdemServico(
            Guid.NewGuid(),
            cliente.Id,
            veiculo.Id,
            request.DescricaoProblema
        );

        await ordemServicoRepository.AdicionarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return os.ParaResponse();
    }
}
