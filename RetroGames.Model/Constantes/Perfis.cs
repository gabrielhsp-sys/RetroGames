namespace RetroGames.Model.Constantes;

/// <summary>
/// Nomes dos perfis (ds_perfil de tb_perfil), usados na claim de perfil do login
/// e nas políticas de autorização.
/// </summary>
public static class Perfis
{
    public const string Administrador = "Administrador";
    public const string Cliente = "Cliente";

    /// <summary>O nome do perfil a partir da chave (co_perfil): 1 → "Administrador", 2 → "Cliente".</summary>
    public static string NomePerfil(this int idPerfil) => idPerfil switch
    {
        PerfilIds.Administrador => Administrador,
        PerfilIds.Cliente => Cliente,
        _ => throw new ArgumentOutOfRangeException(nameof(idPerfil), idPerfil, "Perfil desconhecido."),
    };
}
