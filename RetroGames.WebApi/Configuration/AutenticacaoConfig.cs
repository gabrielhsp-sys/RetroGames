using Microsoft.AspNetCore.Authentication.Cookies;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Configuration;

/// <summary>
/// Sessão por cookie (RS01 e RS02). Uma API não redireciona para uma página de login:
/// sem sessão responde 401 (código 4) e sem permissão, 403 (código 5), sempre no envelope.
/// </summary>
public static class AutenticacaoConfig
{
    public static IServiceCollection AddAutenticacaoPorCookie(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opcoes =>
            {
                opcoes.Cookie.Name = "retrogames.sessao";
                opcoes.Cookie.HttpOnly = true;                 // o JavaScript da página não lê o cookie
                opcoes.Cookie.SameSite = SameSiteMode.Strict;  // não vai em requisições de outros sites (CSRF)
                // Secure segue a requisição: em produção, com HTTPS, o cookie sai marcado como Secure.
                opcoes.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                opcoes.ExpireTimeSpan = TimeSpan.FromHours(2); // 2 horas parado: a sessão expira
                opcoes.SlidingExpiration = true;               // enquanto há uso, a sessão se renova

                opcoes.Events.OnRedirectToLogin = contexto => ResponderAsync(
                    contexto.Response, StatusCodes.Status401Unauthorized, ResponseCodes.NAO_AUTENTICADO);
                opcoes.Events.OnRedirectToAccessDenied = contexto => ResponderAsync(
                    contexto.Response, StatusCodes.Status403Forbidden, ResponseCodes.ACESSO_NEGADO);
            });

        return services;
    }

    /// <summary>Escreve o envelope de erro com o status informado, no lugar do redirecionamento.</summary>
    private static Task ResponderAsync(HttpResponse resposta, int status, (int Code, string DefaultMessage) codigo)
    {
        resposta.StatusCode = status;
        return resposta.WriteAsJsonAsync(new RetroGamesResponseApiModel(codigo.Code, codigo.DefaultMessage));
    }
}
