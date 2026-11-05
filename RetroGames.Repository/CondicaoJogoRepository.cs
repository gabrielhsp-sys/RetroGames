using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_condicao_jogo. Só executa SQL e devolve dados.</summary>
public class CondicaoJogoRepository(RepositoryConfig config) : RepositoryBase(config), ICondicaoJogoRepository
{
    public async Task<List<CondicaoJogo>> ListarAsync()
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT co_condicao_jogo, ds_condicao_jogo FROM tb_condicao_jogo ORDER BY co_condicao_jogo");
        await using var leitor = await comando.ExecuteReaderAsync();

        var condicoes = new List<CondicaoJogo>();
        while (await leitor.ReadAsync())
        {
            condicoes.Add(new CondicaoJogo
            {
                Id = leitor.GetInt32("co_condicao_jogo"),
                Descricao = leitor.GetString("ds_condicao_jogo"),
            });
        }
        return condicoes;
    }
}
