using RetroGames.Model.Constantes;
using RetroGames.Services;
using RetroGames.WebApi.Extensions;

namespace RetroGames.WebApi.Services;

/// <summary>Implementa IUsuarioAtualService com o usuário da requisição atual (claims do cookie).</summary>
public class UsuarioAtualService(IHttpContextAccessor httpContextAccessor) : IUsuarioAtualService
{
    public bool EstaLogado => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public int? Id => httpContextAccessor.HttpContext?.User.ObterIdUsuario();

    public bool EhAdministrador => EstaLogado && httpContextAccessor.HttpContext!.User.IsInRole(Perfis.Administrador);
}
