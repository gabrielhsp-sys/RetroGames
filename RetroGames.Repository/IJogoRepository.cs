using RetroGames.Model.Filtros;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_jogo.</summary>
public interface IJogoRepository
{
    /// <summary>
    /// Os jogos de uma página e o total encontrado com o mesmo filtro.
    /// Cada filtro só vale quando informado: parte do nome (Busca), plataforma e lista de Ids.
    /// Inativos só entram quando IncluirInativos for verdadeiro. IdGenero não é usado aqui.
    /// </summary>
    Task<(List<Jogo> Itens, int Total)> ListarAsync(FiltroJogo filtro);

    /// <summary>O jogo com o id informado, ou nulo se não existir.</summary>
    Task<Jogo?> BuscarPorIdAsync(int id);

    /// <summary>
    /// Os jogos de uma lista de ids, ativos ou não, numa única consulta. Id que não existe
    /// simplesmente não volta. Lista vazia devolve vazio.
    /// </summary>
    Task<List<Jogo>> ListarPorIdsAsync(List<int> ids);

    /// <summary>Grava um novo jogo e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Jogo jogo);

    /// <summary>Atualiza os dados e a situação do jogo (o PUT pode reativá-lo). Não altera a data de cadastro.</summary>
    Task AlterarAsync(Jogo jogo);

    /// <summary>Marca o jogo como inativo (st_ativo). Jogo nunca é apagado.</summary>
    Task DesativarAsync(int id);

    /// <summary>
    /// Baixa a quantidade do estoque só se ainda houver estoque suficiente, num único UPDATE.
    /// Devolve falso quando não baixou (estoque menor que a quantidade, ou jogo inexistente).
    /// A condição no próprio UPDATE impede vender além do estoque em compras simultâneas.
    /// </summary>
    Task<bool> BaixarEstoqueAsync(int id, int quantidade);
}
