namespace RetroGames.Services;

/// <summary>
/// Quem está chamando o Service. O Service não conhece HTTP: a WebApi implementa
/// esta interface lendo o usuário da requisição (cookie de sessão).
/// </summary>
public interface IUsuarioAtualService
{
    /// <summary>Se há usuário logado.</summary>
    bool EstaLogado { get; }

    /// <summary>O id do usuário logado, ou nulo se não há sessão.</summary>
    int? Id { get; }

    /// <summary>Se o usuário logado tem o perfil Administrador.</summary>
    bool EhAdministrador { get; }
}
