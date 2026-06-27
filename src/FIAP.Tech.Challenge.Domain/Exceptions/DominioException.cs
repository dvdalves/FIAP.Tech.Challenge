using System;

namespace FIAP.Tech.Challenge.Domain.Exceptions;

public class DominioException : Exception
{
    public DominioException(string message) : base(message)
    {
    }

    public DominioException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
