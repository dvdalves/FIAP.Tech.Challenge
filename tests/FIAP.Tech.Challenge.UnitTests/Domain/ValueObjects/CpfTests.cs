using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.UnitTests.Domain.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CriarCpf_ComValorVazioOuNulo_DeveLancarDominioExceptionIndicandoVazio(string? valorInvalido)
    {
        // Act
        var act = () => new Cpf(valorInvalido!);

        // Assert
        act.Should().Throw<DominioException>()
           .WithMessage("CPF não pode ser vazio.");
    }

    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("11111111111")]
    public void CriarCpf_ComValorInvalido_DeveLancarDominioExceptionIndicandoInvalido(string valorInvalido)
    {
        // Act
        var act = () => new Cpf(valorInvalido);

        // Assert
        act.Should().Throw<DominioException>()
           .WithMessage("*inválido*");
    }

    [Theory]
    [InlineData("12345678909", "12345678909")]
    [InlineData("123.456.789-09", "12345678909")]
    public void CriarCpf_ComValorValido_DeveCriarInstanciaSucesso(string valorValido, string valorEsperado)
    {
        // Act
        var cpf = new Cpf(valorValido);

        // Assert
        cpf.Valor.Should().Be(valorEsperado);
    }
}
