using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_jogo.</summary>
public interface IJogoRepository
{
    /// <summary>
    /// Os jogos de uma página e o total encontrado com o mesmo filtro.
    /// Cada filtro só vale quando informado: parte do nome (busca), plataforma e lista de ids.
    /// Inativos só entram quando incluirInativos for verdadeiro.
    /// </summary>
    Task<(List<Jogo> Itens, int Total)> ListarAsync(
        string? busca, int? idPlataforma, List<int>? ids, bool incluirInativos,
        int pagina, int tamanhoPagina);

    /// <summary>O jogo com o id informado, ou nulo se não existir.</summary>
    Task<Jogo?> BuscarPorIdAsync(int id);

    /// <summary>Grava um novo jogo e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Jogo jogo);

    /// <summary>Atualiza os dados do jogo (não altera situação nem data de cadastro).</summary>
    Task AlterarAsync(Jogo jogo);

    /// <summary>Marca o jogo como inativo (st_ativo). Jogo nunca é apagado.</summary>
    Task DesativarAsync(int id);
}
