using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Domain.Aggregates;

public class PecaAggregateTests
{
    private readonly Guid _pecaIdValido = Guid.NewGuid();

    [Fact]
    public void CriarPeca_ComDadosValidos_DeveSucesso()
    {
        // Act
        var peca = new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, 10);

        // Assert
        peca.Id.Should().Be(_pecaIdValido);
        peca.Nome.Should().Be("Filtro de Óleo");
        peca.Preco.Should().Be(45.90m);
        peca.QuantidadeEstoque.Should().Be(10);
    }

    [Fact]
    public void CriarPeca_ComIdVazio_DeveLancarDominioException()
    {
        // Act
        var act = () => new Peca(Guid.Empty, "Filtro de Óleo", 45.90m, 10);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Id da peça inválido.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarPeca_ComNomeInvalido_DeveLancarDominioException(string? nomeInvalido)
    {
        // Act
        var act = () => new Peca(_pecaIdValido, nomeInvalido!, 45.90m, 10);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("O nome da peça é obrigatório.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CriarPeca_ComPrecoInvalido_DeveLancarDominioException(decimal precoInvalido)
    {
        // Act
        var act = () => new Peca(_pecaIdValido, "Filtro de Óleo", precoInvalido, 10);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("O preço da peça deve ser maior que zero.");
    }

    [Fact]
    public void CriarPeca_ComQuantidadeNegativa_DeveLancarDominioException()
    {
        // Act
        var act = () => new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, -1);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("A quantidade em estoque não pode ser negativa.");
    }

    [Fact]
    public void AjustarEstoque_ComQuantidadeNegativa_DeveLancarDominioException()
    {
        // Arrange
        var peca = new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, 10);

        // Act
        var act = () => peca.AjustarEstoque(-5);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("A quantidade em estoque não pode ser negativa.");
    }

    [Fact]
    public void AjustarEstoque_ComQuantidadeValida_DeveAtualizarEstoque()
    {
        // Arrange
        var peca = new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, 10);

        // Act
        peca.AjustarEstoque(25);

        // Assert
        peca.QuantidadeEstoque.Should().Be(25);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void DeduzirEstoque_ComQuantidadeInvalida_DeveLancarDominioException(int qtdInvalida)
    {
        // Arrange
        var peca = new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, 10);

        // Act
        var act = () => peca.DeduzirEstoque(qtdInvalida);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("A quantidade a deduzir deve ser maior que zero.");
    }

    [Fact]
    public void DeduzirEstoque_ComQuantidadeMaiorQueEstoque_DeveLancarDominioException()
    {
        // Arrange
        var peca = new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, 5);

        // Act
        var act = () => peca.DeduzirEstoque(10);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public void DeduzirEstoque_ComQuantidadeValida_DeveReduzirEstoque()
    {
        // Arrange
        var peca = new Peca(_pecaIdValido, "Filtro de Óleo", 45.90m, 10);

        // Act
        peca.DeduzirEstoque(4);

        // Assert
        peca.QuantidadeEstoque.Should().Be(6);
    }
}