namespace RetroGames.WebApi.Configuration;

/// <summary>Nomes das políticas de autorização, usados em [Authorize(Policy = ...)].</summary>
public static class Politicas
{
    /// <summary>Exige sessão com o perfil Administrador (RS03: política SomenteAdministrador).</summary>
    public const string Administrador = "SomenteAdministrador";
}
