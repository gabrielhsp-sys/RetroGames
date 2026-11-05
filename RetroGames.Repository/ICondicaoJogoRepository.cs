using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com o SQL da tabela tb_condicao_jogo (somente leitura).</summary>
public interface ICondicaoJogoRepository
{
    /// <summary>Todas as condições de jogo, na ordem da chave.</summary>
    Task<List<CondicaoJogo>> ListarAsync();
}
