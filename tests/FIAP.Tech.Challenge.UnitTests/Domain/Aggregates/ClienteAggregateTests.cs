using System;
using Xunit;
using FluentAssertions;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.UnitTests.Domain.Aggregates;

public class ClienteAggregateTests
{
    private readonly Cpf _cpfValido = new("12345678909");

    [Fact]
    public void CriarCliente_ComDadosValidos_DeveInstanciarSucesso()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var cliente = new Cliente(id, "Carlos Souza", _cpfValido, "carlos@email.com", "11999998888");

        // Assert
        cliente.Id.Should().Be(id);
        cliente.Nome.Should().Be("Carlos Souza");
        cliente.Cpf.Should().Be(_cpfValido);
        cliente.Email.Should().Be("carlos@email.com");
        cliente.Telefone.Should().Be("11999998888");
    }

    [Fact]
    public void CriarCliente_ComEmailInvalido_DeveLancarDominioException()
    {
        // Act
        var act = () => new Cliente(Guid.NewGuid(), "Carlos", _cpfValido, "carlos-email.com", "11999998888");

        // Assert
        act.Should().Throw<DominioException>().WithMessage("*E-mail*inválido*");
    }

    [Fact]
    public void AlterarNome_ComNomeVazio_DeveLancarDominioException()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "Carlos", _cpfValido, "carlos@email.com", "11999998888");

        // Act
        var act = () => cliente.AlterarNome("");

        // Assert
        act.Should().Throw<DominioException>().WithMessage("*Nome*obrigatório*");
    }
}
