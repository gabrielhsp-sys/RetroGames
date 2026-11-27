using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_usuario.</summary>
public interface IUsuarioRepository
{
    /// <summary>O usuário com o e-mail informado, ou nulo se não existir.</summary>
    Task<Usuario?> BuscarPorEmailAsync(string email);

    /// <summary>O usuário com o id informado, ou nulo se não existir.</summary>
    Task<Usuario?> BuscarPorIdAsync(int id);

    /// <summary>Se já há algum usuário com o perfil Administrador.</summary>
    Task<bool> ExisteAdministradorAsync();

    /// <summary>Os usuários de uma lista de ids, numa única consulta. Lista vazia devolve vazio.</summary>
    Task<List<Usuario>> ListarPorIdsAsync(List<int> ids);

    /// <summary>Grava um novo usuário e devolve o id gerado. E-mail repetido: RegistroDuplicadoException.</summary>
    Task<int> InserirAsync(Usuario usuario);
}
