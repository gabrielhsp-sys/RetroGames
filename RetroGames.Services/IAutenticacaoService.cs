using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Conferência de login, cadastro de cliente (C01) e criação do administrador inicial (RS04 e RS05).</summary>
public interface IAutenticacaoService
{
    /// <summary>
    /// O usuário, se o e-mail existe, a senha confere com o hash e ele está ativo.
    /// Nos três casos de falha devolve nulo: quem chama não descobre qual deles foi.
    /// </summary>
    Task<Usuario?> AutenticarAsync(string email, string senha);

    /// <summary>
    /// C01: cria o usuário como Cliente ativo, com a senha guardada só como hash (RS04),
    /// e o devolve com o id gerado. E-mail já cadastrado: EMAIL_JA_CADASTRADO (409, código 106).
    /// </summary>
    Task<Usuario> CadastrarAsync(Usuario usuario, string senha);

    /// <summary>Cria o administrador com este e-mail e senha se ainda não houver nenhum. Senão, não faz nada.</summary>
    Task GarantirAdministradorInicialAsync(string email, string senha);
}
