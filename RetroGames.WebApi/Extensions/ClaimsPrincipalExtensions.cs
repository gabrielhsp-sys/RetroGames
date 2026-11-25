using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;

namespace RetroGames.WebApi.Extensions;

/// <summary>
/// Claims da sessão: o que vai dentro do cookie (id, nome, e-mail e perfil) e a leitura
/// do usuário logado. Senha e hash nunca viram claim.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>A identidade que o SignInAsync grava no cookie.</summary>
    public static ClaimsPrincipal ParaClaimsPrincipal(this Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.IdPerfil.NomePerfil()),
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    /// <summary>O id do usuário logado, ou nulo se não há sessão.</summary>
    public static int? ObterIdUsuario(this ClaimsPrincipal usuario)
    {
        return int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }
}
