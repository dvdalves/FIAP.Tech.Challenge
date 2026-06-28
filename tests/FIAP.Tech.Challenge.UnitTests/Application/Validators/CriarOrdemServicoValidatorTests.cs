using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.Validators;
using FluentAssertions;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.Validators;

public class CriarOrdemServicoValidatorTests
{
    private readonly CriarOrdemServicoValidator _validator = new();

    [Fact]
    public void Validar_ComCamposVazios_DeveRetornarErrosDeValidacao()
    {
        // Arrange
        var requestInvalida = new CriarOrdemServicoRequest();

        // Act
        var resultado = _validator.Validate(requestInvalida);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(x => x.PropertyName == nameof(CriarOrdemServicoRequest.ClienteNome));
        resultado.Errors.Should().Contain(x => x.PropertyName == nameof(CriarOrdemServicoRequest.ClienteCpf));
        resultado.Errors.Should().Contain(x => x.PropertyName == nameof(CriarOrdemServicoRequest.VeiculoPlaca));
    }

    [Fact]
    public void Validar_ComTodosCamposValidos_DevePassarNaValidacao()
    {
        // Arrange
        var requestValida = new CriarOrdemServicoRequest
        {
            ClienteNome = "Guilherme",
            ClienteCpf = "12345678909",
            ClienteEmail = "gui@email.com",
            ClienteTelefone = "11999998888",
            VeiculoPlaca = "ABC1234",
            VeiculoMarca = "Fiat",
            VeiculoModelo = "Uno",
            VeiculoAno = 2012,
            DescricaoProblema = "Revisão preventiva"
        };

        // Act
        var resultado = _validator.Validate(requestValida);

        // Assert
        resultado.IsValid.Should().BeTrue();
        resultado.Errors.Should().BeEmpty();
    }
}