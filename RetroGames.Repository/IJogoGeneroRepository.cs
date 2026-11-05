using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_jogo_genero.</summary>
public interface IJogoGeneroRepository
{
    /// <summary>Todos os vínculos entre jogos e gêneros.</summary>
    Task<List<JogoGenero>> ListarAsync();

    /// <summary>Os vínculos de uma lista de jogos, numa única consulta. Lista vazia devolve vazio.</summary>
    Task<List<JogoGenero>> ListarPorJogosAsync(List<int> idsJogo);

    /// <summary>Grava os vínculos de um jogo com os seus gêneros.</summary>
    Task InserirAsync(int idJogo, List<int> idsGenero);

    /// <summary>Apaga os vínculos de um jogo, para regravá-los na alteração.</summary>
    Task RemoverPorJogoAsync(int idJogo);
}
