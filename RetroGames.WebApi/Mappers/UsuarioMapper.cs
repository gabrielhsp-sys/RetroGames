using System.Security.Claims;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Extensions;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversão, escrita à mão, do usuário nos dados da sessão que saem no JSON. Nunca o hash.</summary>
public static class UsuarioMapper
{
    /// <summary>Usuário recém-autenticado (login) → ApiModel.</summary>
    public static UsuarioLogadoResponseApiModel ParaUsuarioLogadoResponse(this Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Perfil = usuario.IdPerfil.NomePerfil(),
    };

    /// <summary>Usuário da sessão, lido das claims do cookie (GET eu) → ApiModel.</summary>
    public static UsuarioLogadoResponseApiModel ParaUsuarioLogadoResponse(this ClaimsPrincipal usuario) => new()
    {
        Id = usuario.ObterIdUsuario() ?? 0,
        Nome = usuario.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
        Email = usuario.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
        Perfil = usuario.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
    };
}
