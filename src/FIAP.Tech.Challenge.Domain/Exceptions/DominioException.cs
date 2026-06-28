using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public class DominioException : Exception
{
    public DominioException(string message) : base(message)
    {
    }

    public DominioException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
