namespace RetroGames.WebApi.ApiModels;

/// <summary>O usuário da sessão, como sai no login e em GET /api/Autenticacao/eu. Nunca leva a senha nem o hash.</summary>
public class UsuarioLogadoResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>"Administrador" ou "Cliente".</summary>
    public string Perfil { get; set; } = string.Empty;
}
