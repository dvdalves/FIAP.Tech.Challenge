using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.UnitTests.Domain.ValueObjects;

public class CnpjTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CriarCnpj_ComValorVazioOuNulo_DeveLancarDominioExceptionIndicandoVazio(string? valorInvalido)
    {
        // Act
        var act = () => new Cnpj(valorInvalido!);

        // Assert
        act.Should().Throw<DominioException>()
           .WithMessage("CNPJ não pode ser vazio.");
    }

    [Theory]
    [InlineData("12.345.678/0001-00")]
    [InlineData("11111111111111")]
    public void CriarCnpj_ComValorInvalido_DeveLancarDominioExceptionIndicandoInvalido(string valorInvalido)
    {
        // Act
        var act = () => new Cnpj(valorInvalido);

        // Assert
        act.Should().Throw<DominioException>()
           .WithMessage("*inválido*");
    }

    [Theory]
    [InlineData("60701190000104", "60701190000104")]
    [InlineData("60.701.190/0001-04", "60701190000104")]
    public void CriarCnpj_ComValorValido_DeveCriarInstanciaSucesso(string valorValido, string valorEsperado)
    {
        // Act
        var cnpj = new Cnpj(valorValido);

        // Assert
        cnpj.ToString().Should().Be(valorEsperado);
    }
}
