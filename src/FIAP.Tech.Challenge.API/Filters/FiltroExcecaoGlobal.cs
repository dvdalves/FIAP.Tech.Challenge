using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.API.Filters;

public class FiltroExcecaoGlobal : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DominioException dominioException)
        {
            context.Result = new ObjectResult(new { erro = dominioException.Message })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
        }
        else
        {
            // Erro genérico para exceções não tratadas
            context.Result = new ObjectResult(new { erro = "Ocorreu um erro interno no servidor.", detalhe = context.Exception.Message })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
        }
    }
}
