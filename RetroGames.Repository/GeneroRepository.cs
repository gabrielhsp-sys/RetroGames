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
            generos.Add(new Genero
            {
                Id = leitor.GetInt32("co_seq_genero"),
                Nome = leitor.GetString("no_genero"),
            });
        }
        return generos;
    }
}
