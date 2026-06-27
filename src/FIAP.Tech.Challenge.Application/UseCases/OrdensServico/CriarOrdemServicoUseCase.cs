using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class CriarOrdemServicoUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarOrdemServicoUseCase(
        IClienteRepository clienteRepository,
        IVeiculoRepository veiculoRepository,
        IOrdemServicoRepository ordemServicoRepository,
        IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _ordemServicoRepository = ordemServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrdemServicoResponse> ExecutarAsync(CriarOrdemServicoRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Validar e Criar Objetos de Valor
        var cpf = new Cpf(request.ClienteCpf);
        var placa = new Placa(request.VeiculoPlaca);

        // 2. Obter ou Criar Cliente
        var cliente = await _clienteRepository.ObterPorCpfAsync(cpf, cancellationToken);
        if (cliente == null)
        {
            cliente = new Cliente(
                Guid.NewGuid(), 
                request.ClienteNome, 
                cpf, 
                request.ClienteEmail, 
                request.ClienteTelefone
            );
            await _clienteRepository.AdicionarAsync(cliente, cancellationToken);
        }

        // 3. Obter ou Criar Veículo
        var veiculo = await _veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (veiculo == null)
        {
            veiculo = new Veiculo(
                Guid.NewGuid(), 
                placa, 
                request.VeiculoMarca, 
                request.VeiculoModelo, 
                request.VeiculoAno, 
                cliente.Id
            );
            await _veiculoRepository.AdicionarAsync(veiculo, cancellationToken);
        }
        else if (veiculo.ClienteId != cliente.Id)
        {
            throw new DominioException("Este veículo já está cadastrado para outro cliente.");
        }

        // 4. Criar Ordem de Serviço
        var ordemServico = new OrdemServico(
            Guid.NewGuid(), 
            cliente.Id, 
            veiculo.Id, 
            request.DescricaoProblema
        );

        await _ordemServicoRepository.AdicionarAsync(ordemServico, cancellationToken);

        // 5. Commit
        await _unitOfWork.CommitAsync(cancellationToken);

        return ordemServico.ParaResponse();
    }
}
