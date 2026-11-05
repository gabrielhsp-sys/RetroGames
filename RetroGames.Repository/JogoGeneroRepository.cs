using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_jogo_genero. Só executa SQL e devolve dados.</summary>
public class JogoGeneroRepository(RepositoryConfig config) : RepositoryBase(config), IJogoGeneroRepository
{
    public async Task<List<JogoGenero>> ListarAsync()
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT co_jogo, co_genero FROM tb_jogo_genero ORDER BY co_jogo, co_genero");
        await using var leitor = await comando.ExecuteReaderAsync();

        return await LerVinculosAsync(leitor);
    }

    public async Task<List<JogoGenero>> ListarPorJogosAsync(List<int> idsJogo)
    {
        // Sem ids, não há vínculo a buscar; "IN ()" seria SQL inválido.
        if (idsJogo.Count == 0)
        {
            return new List<JogoGenero>();
        }

        await using var conexao = await CriarConexaoAsync();
        // Só os nomes dos parâmetros (@id0, @id1...) entram no texto; os valores vão como parâmetro.
        var nomes = string.Join(", ", Enumerable.Range(0, idsJogo.Count).Select(i => "@id" + i));
        await using var comando = CriarComando(conexao,
            "SELECT co_jogo, co_genero FROM tb_jogo_genero " +
            "WHERE co_jogo IN (" + nomes + ") ORDER BY co_jogo, co_genero");
        for (var i = 0; i < idsJogo.Count; i++)
        {
            AdicionarParametro(comando, "@id" + i, idsJogo[i]);
        }
        await using var leitor = await comando.ExecuteReaderAsync();

        return await LerVinculosAsync(leitor);
    }

    public async Task InserirAsync(int idJogo, List<int> idsGenero)
    {
        if (idsGenero.Count == 0)
        {
            return;
        }

        await using var conexao = await CriarConexaoAsync();
        // Um único INSERT com uma linha (@idJogo, @idGeneroN) por gênero.
        var linhas = string.Join(", ",
            Enumerable.Range(0, idsGenero.Count).Select(i => "(@idJogo, @idGenero" + i + ")"));
        await using var comando = CriarComando(conexao,
            "INSERT INTO tb_jogo_genero (co_jogo, co_genero) VALUES " + linhas);
        AdicionarParametro(comando, "@idJogo", idJogo);
        for (var i = 0; i < idsGenero.Count; i++)
        {
            AdicionarParametro(comando, "@idGenero" + i, idsGenero[i]);
        }

        await ExecutarGravacaoAsync(comando);
    }

    public async Task RemoverPorJogoAsync(int idJogo)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "DELETE FROM tb_jogo_genero WHERE co_jogo = @idJogo");
        AdicionarParametro(comando, "@idJogo", idJogo);

        await comando.ExecuteNonQueryAsync();
    }

    /// <summary>Converte cada linha do leitor num JogoGenero, coluna por coluna.</summary>
    private static async Task<List<JogoGenero>> LerVinculosAsync(MySqlDataReader leitor)
    {
        var vinculos = new List<JogoGenero>();
        while (await leitor.ReadAsync())
        {
            vinculos.Add(new JogoGenero
            {
                IdJogo = leitor.GetInt32("co_jogo"),
                IdGenero = leitor.GetInt32("co_genero"),
            });
        }
        return vinculos;
    }
}
