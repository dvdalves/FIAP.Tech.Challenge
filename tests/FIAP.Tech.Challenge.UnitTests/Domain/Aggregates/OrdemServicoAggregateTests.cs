using System;
using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.UnitTests.Domain.Aggregates;

public class OrdemServicoAggregateTests
{
    [Fact]
    public void CriarOrdemServico_ComDadosValidos_DeveInicializarComoRecebida()
    {
        // Act
        var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Troca de amortecedores");

        // Assert
        os.Status.Should().Be(StatusOrdemServico.Recebida);
        os.ValorTotal.Should().Be(0);
        os.DataCriacao.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void DefinirOrcamento_ForaDoStatusEmDiagnostico_DeveLancarDominioException()
    {
        // Arrange
        var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Troca de amortecedores");

        // Act
        var act = () => os.DefinirOrcamento(150.00m);

        // Assert
        act.Should().Throw<DominioException>().WithMessage("*Diagnóstico*");
    }

    [Fact]
    public void AvancarStatus_ParaAguardandoAprovacaoSemOrcamento_DeveLancarDominioException()
    {
        // Arrange
        var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Troca de amortecedores");
        os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);

        // Act
        var act = () => os.AtualizarStatus(StatusOrdemServico.AguardandoAprovacao);

        // Assert
        act.Should().Throw<DominioException>().WithMessage("*orçamento definido*");
    }

    [Fact]
    public void FluxoCompletoStatus_ComOrcamento_DeveTransitarCorretamente()
    {
        // Arrange
        var os = new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Troca de amortecedores");

        // Act & Assert
        // Recebida -> EmDiagnostico
        os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);
        os.Status.Should().Be(StatusOrdemServico.EmDiagnostico);

        // Define orçamento
        os.DefinirOrcamento(350.00m);
        os.ValorTotal.Should().Be(350.00m);

        // EmDiagnostico -> AguardandoAprovacao
        os.AtualizarStatus(StatusOrdemServico.AguardandoAprovacao);
        os.Status.Should().Be(StatusOrdemServico.AguardandoAprovacao);

        // AguardandoAprovacao -> EmExecucao
        os.AtualizarStatus(StatusOrdemServico.EmExecucao);
        os.Status.Should().Be(StatusOrdemServico.EmExecucao);

        // EmExecucao -> Finalizada
        os.AtualizarStatus(StatusOrdemServico.Finalizada);
        os.Status.Should().Be(StatusOrdemServico.Finalizada);
        os.DataFinalizacao.Should().NotBeNull();
    }
}
