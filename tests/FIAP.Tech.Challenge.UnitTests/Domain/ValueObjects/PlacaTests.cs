using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Domain.ValueObjects;

public class PlacaTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CriarPlaca_ComValorVazioOuNulo_DeveLancarDominioExceptionIndicandoVazia(string? valorInvalido)
    {
        // Act
        var act = () => new Placa(valorInvalido!);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("Placa do veículo não pode ser vazia.");
    }

    [Theory]
    [InlineData("ABC-123")]
    [InlineData("ABC12345")]
    public void CriarPlaca_ComValorInvalido_DeveLancarDominioExceptionIndicandoInvalida(string valorInvalido)
    {
        // Act
        var act = () => new Placa(valorInvalido);

        // Assert
        act.Should().Throw<DominioException>()
            .WithMessage("*inválida*");
    }

    [Theory]
    [InlineData("ABC-1234", "ABC1234")]
    [InlineData("abc1234", "ABC1234")]
    [InlineData("ABC1D23", "ABC1D23")]
    [InlineData("abc1d23", "ABC1D23")]
    public void CriarPlaca_ComValorValido_DeveCriarInstanciaSucesso(string valorValido, string valorEsperado)
    {
        // Act
        var placa = new Placa(valorValido);

        // Assert
        placa.Valor.Should().Be(valorEsperado);
    }
}