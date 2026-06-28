using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Domain.Aggregates;

public class VeiculoAggregateTests
{
    private readonly Guid _clienteIdValido = Guid.NewGuid();
    private readonly Placa _placaValida = new("AAA-1234");
    private readonly Guid _veiculoIdValido = Guid.NewGuid();

    [Fact]
    public void CriarVeiculo_ComDadosValidos_DeveSucesso()
    {
        // Act
        var veiculo = new Veiculo(_veiculoIdValido, _placaValida, "Toyota", "Corolla", 2022, _clienteIdValido);

        // Assert
        veiculo.Id.Should().Be(_veiculoIdValido);
        veiculo.Placa.Should().Be(_placaValida);
        veiculo.Marca.Should().Be("Toyota");
        veiculo.Modelo.Should().Be("Corolla");
        veiculo.Ano.Should().Be(2022);
        veiculo.ClienteId.Should().Be(_clienteIdValido);
    }

    [Fact]
    public void CriarVeiculo_ComIdVazio_DeveLancarDominioException()
    {
        // Act
        var act = () => new Veiculo(Guid.Empty, _placaValida, "Ford", "Ka", 2018, _clienteIdValido);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Id do veículo inválido.");
    }

    [Fact]
    public void CriarVeiculo_ComClienteIdVazio_DeveLancarDominioException()
    {
        // Act
        var act = () => new Veiculo(_veiculoIdValido, _placaValida, "Ford", "Ka", 2018, Guid.Empty);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Id do cliente associado ao veículo é obrigatório.");
    }

    [Fact]
    public void AlterarPlaca_Nula_DeveLancarDominioException()
    {
        // Arrange
        var veiculo = new Veiculo(_veiculoIdValido, _placaValida, "Ford", "Ka", 2018, _clienteIdValido);

        // Act
        var act = () => veiculo.AlterarPlaca(null!);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Placa do veículo é obrigatória.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AlterarMarca_Invalida_DeveLancarDominioException(string? marcaInvalida)
    {
        // Arrange
        var veiculo = new Veiculo(_veiculoIdValido, _placaValida, "Ford", "Ka", 2018, _clienteIdValido);

        // Act
        var act = () => veiculo.AlterarMarca(marcaInvalida!);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Marca do veículo é obrigatória.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AlterarModelo_Invalido_DeveLancarDominioException(string? modeloInvalido)
    {
        // Arrange
        var veiculo = new Veiculo(_veiculoIdValido, _placaValida, "Ford", "Ka", 2018, _clienteIdValido);

        // Act
        var act = () => veiculo.AlterarModelo(modeloInvalido!);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Modelo do veículo é obrigatório.");
    }

    [Theory]
    [InlineData(1800)]
    [InlineData(2100)]
    public void AlterarAno_Invalido_DeveLancarDominioException(int anoInvalido)
    {
        // Arrange
        var veiculo = new Veiculo(_veiculoIdValido, _placaValida, "Ford", "Ka", 2018, _clienteIdValido);

        // Act
        var act = () => veiculo.AlterarAno(anoInvalido);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Ano do veículo deve estar entre 1886 e *");
    }

    [Fact]
    public void AlterarDados_ComValoresValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var veiculo = new Veiculo(_veiculoIdValido, _placaValida, "Ford", "Ka", 2018, _clienteIdValido);
        var novaPlaca = new Placa("BBB-5678");

        // Act
        veiculo.AlterarPlaca(novaPlaca);
        veiculo.AlterarMarca(" Chevrolet ");
        veiculo.AlterarModelo(" Onix ");
        veiculo.AlterarAno(2020);

        // Assert
        veiculo.Placa.Should().Be(novaPlaca);
        veiculo.Marca.Should().Be("Chevrolet");
        veiculo.Modelo.Should().Be("Onix");
        veiculo.Ano.Should().Be(2020);
    }
}