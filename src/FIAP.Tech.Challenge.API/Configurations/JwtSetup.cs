using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.API.Configurations;

[ExcludeFromCodeCoverage]
public static class JwtSetup
{
    private const string SecretKey = "SuperSecretSecurityKeyOficinaMecanica2026!";

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var key = Encoding.ASCII.GetBytes(SecretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();

        return services;
    }
}
