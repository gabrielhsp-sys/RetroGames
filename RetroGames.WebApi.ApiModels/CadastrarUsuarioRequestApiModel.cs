namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Corpo de POST /api/Autenticacao/cadastrar (C01). Só nome, e-mail e senha:
/// perfil, situação e data quem define é o Service (Cliente, ativo, agora em UTC).
/// </summary>
public class CadastrarUsuarioRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
