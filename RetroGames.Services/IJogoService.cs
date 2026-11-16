using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Catálogo e regras de negócio de jogos (RN03 a RN05).</summary>
public interface IJogoService
{
    /// <summary>
    /// Uma página do catálogo, com o detalhe de cada jogo, e o total encontrado.
    /// Cada filtro só vale quando informado. Por enquanto lista só jogos ativos.
    /// </summary>
    Task<(List<JogoDetalheServiceModel> Itens, int Total)> ListarAsync(
        string? busca, int? idPlataforma, int? idGenero, int pagina, int tamanhoPagina);

    /// <summary>O detalhe do jogo. Se não existir, JOGO_NAO_ENCONTRADO.</summary>
    Task<JogoDetalheServiceModel> BuscarPorIdAsync(int id);

    /// <summary>Grava o jogo (ativo) e os seus gêneros numa transação e o devolve com o id gerado.</summary>
    Task<Jogo> InserirAsync(Jogo jogo, List<int> idsGenero);

    /// <summary>Altera o jogo e troca os seus gêneros numa transação.</summary>
    Task AlterarAsync(Jogo jogo, List<int> idsGenero);

    /// <summary>Marca o jogo como inativo. Jogo nunca é apagado (RN03).</summary>
    Task DesativarAsync(int id);
}
