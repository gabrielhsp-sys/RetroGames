using Microsoft.AspNetCore.Authorization;
using RetroGames.Model.Constantes;

namespace RetroGames.WebApi.Configuration;

/// <summary>
/// API segura por padrão (RS03): a FallbackPolicy exige login em toda rota que não diga
/// outra coisa. Rota pública recebe [AllowAnonymous]; gravação, a política SomenteAdministrador.
/// </summary>
public static class AutorizacaoConfig
{
    public static IServiceCollection AddAutorizacao(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Politicas.Administrador, politica => politica.RequireRole(Perfis.Administrador));

        return services;
    }
}
