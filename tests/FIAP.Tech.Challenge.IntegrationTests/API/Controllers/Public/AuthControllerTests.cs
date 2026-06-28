using System.Net;
using FluentAssertions;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.API.Controllers.Public;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GerarTokenTeste_DeveRetornarTokenJWTComStatus200Ok()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/public/auth/token?usuario=test_user&perfil=Admin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }
}