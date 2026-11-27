using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_genero.</summary>
public interface IGeneroRepository
{
    /// <summary>Todos os gêneros, ordenados por nome.</summary>
    Task<List<Genero>> ListarAsync();

    /// <summary>O gênero com o id informado, ou nulo se não existir.</summary>
    Task<Genero?> BuscarPorIdAsync(int id);

    /// <summary>Se já há gênero com o nome informado, ignorando o de id idIgnorar (na alteração).</summary>
    Task<bool> ExisteComNomeAsync(string nome, int? idIgnorar = null);

    /// <summary>Grava um novo gênero e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Genero genero);

    /// <summary>Atualiza o nome do gênero.</summary>
    Task AlterarAsync(Genero genero);

    /// <summary>Apaga o gênero pelo id.</summary>
    Task ExcluirAsync(int id);
}
