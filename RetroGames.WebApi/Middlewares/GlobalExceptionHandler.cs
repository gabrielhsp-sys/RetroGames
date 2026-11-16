using Microsoft.AspNetCore.Diagnostics;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Middlewares;

/// <summary>
/// Único ponto que transforma exceção em resposta HTTP, sempre com o envelope de erro
/// (sem "data"). Por isso nenhum controller tem try/catch.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        RetroGamesResponseApiModel resposta;

        if (exception is BusinessRuleException erroDeNegocio)
        {
            // Regra violada: status e código vêm do catálogo BusinessErrors (400, 404 ou 409; 100 em diante).
            httpContext.Response.StatusCode = (int)erroDeNegocio.Erro.Status;
            resposta = new RetroGamesResponseApiModel(erroDeNegocio.Erro.Code, erroDeNegocio.Message);
        }
        else
        {
            // Erro inesperado: o detalhe completo vai só para o log. A resposta é genérica,
            // sem SQL, nome de tabela ou caminho de arquivo.
            logger.LogError(exception, "Erro inesperado em {Metodo} {Caminho}.",
                httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            resposta = new RetroGamesResponseApiModel(ResponseCodes.ERRO.Code, ResponseCodes.ERRO.DefaultMessage);
        }

        await httpContext.Response.WriteAsJsonAsync(resposta, cancellationToken);
        return true;
    }
}
