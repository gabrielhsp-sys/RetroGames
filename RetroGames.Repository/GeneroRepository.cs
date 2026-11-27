using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_genero. Só executa SQL e devolve dados.</summary>
public class GeneroRepository(RepositoryConfig config) : RepositoryBase(config), IGeneroRepository
{
    public async Task<List<Genero>> ListarAsync()
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT co_seq_genero, no_genero FROM tb_genero ORDER BY no_genero");
        await using var leitor = await comando.ExecuteReaderAsync();

        var generos = new List<Genero>();
        while (await leitor.ReadAsync())
        {
            generos.Add(Mapear(leitor));
        }
        return generos;
    }

    public async Task<Genero?> BuscarPorIdAsync(int id)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT co_seq_genero, no_genero FROM tb_genero WHERE co_seq_genero = @id");
        AdicionarParametro(comando, "@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<bool> ExisteComNomeAsync(string nome, int? idIgnorar = null)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT COUNT(*) FROM tb_genero " +
            "WHERE no_genero = @nome AND (@idIgnorar IS NULL OR co_seq_genero <> @idIgnorar)");
        AdicionarParametro(comando, "@nome", nome);
        AdicionarParametro(comando, "@idIgnorar", idIgnorar);

        return Convert.ToInt64(await comando.ExecuteScalarAsync()) > 0;
    }

    public async Task<int> InserirAsync(Genero genero)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "INSERT INTO tb_genero (no_genero) VALUES (@nome)");
        AdicionarParametro(comando, "@nome", genero.Nome);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    public async Task AlterarAsync(Genero genero)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "UPDATE tb_genero SET no_genero = @nome WHERE co_seq_genero = @id");
        AdicionarParametro(comando, "@nome", genero.Nome);
        AdicionarParametro(comando, "@id", genero.Id);

        await ExecutarGravacaoAsync(comando);
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "DELETE FROM tb_genero WHERE co_seq_genero = @id");
        AdicionarParametro(comando, "@id", id);

        await comando.ExecuteNonQueryAsync();
    }

    /// <summary>Converte a linha atual do leitor num Genero, coluna por coluna.</summary>
    private static Genero Mapear(MySqlDataReader leitor)
    {
        return new Genero
        {
            Id = leitor.GetInt32("co_seq_genero"),
            Nome = leitor.GetString("no_genero"),
        };
    }
}
