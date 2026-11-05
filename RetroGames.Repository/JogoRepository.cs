using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_jogo. Só executa SQL e devolve dados.</summary>
public class JogoRepository(RepositoryConfig config) : RepositoryBase(config), IJogoRepository
{
    private const string Colunas =
        "co_seq_jogo, no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, " +
        "st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo";

    public async Task<(List<Jogo> Itens, int Total)> ListarAsync(
        string? busca, int? idPlataforma, List<int>? ids, bool incluirInativos,
        int pagina, int tamanhoPagina)
    {
        // Filtro por lista vazia não encontra nenhum jogo; "IN ()" seria SQL inválido.
        if (ids is { Count: 0 })
        {
            return (new List<Jogo>(), 0);
        }

        // Mesmo filtro para a página e para a contagem. Cada filtro só vale quando informado.
        var filtro =
            "WHERE (@busca IS NULL OR no_jogo LIKE CONCAT('%', @busca, '%')) " +
            "AND (@idPlataforma IS NULL OR co_plataforma = @idPlataforma) " +
            "AND (@incluirInativos = TRUE OR st_ativo = TRUE)";
        if (ids != null)
        {
            // Só os nomes dos parâmetros (@id0, @id1...) entram no texto; os valores vão como parâmetro.
            filtro += " AND co_seq_jogo IN (" + NomesDeParametros("@id", ids.Count) + ")";
        }

        await using var conexao = await CriarConexaoAsync();

        await using var comandoTotal = CriarComando(conexao, "SELECT COUNT(*) FROM tb_jogo " + filtro);
        AdicionarParametrosDoFiltro(comandoTotal);
        var total = Convert.ToInt32(await comandoTotal.ExecuteScalarAsync());

        await using var comando = CriarComando(conexao,
            "SELECT " + Colunas + " FROM tb_jogo " + filtro +
            " ORDER BY no_jogo, co_seq_jogo LIMIT @tamanhoPagina OFFSET @deslocamento");
        AdicionarParametrosDoFiltro(comando);
        AdicionarParametro(comando, "@tamanhoPagina", tamanhoPagina);
        AdicionarParametro(comando, "@deslocamento", (pagina - 1) * tamanhoPagina);
        await using var leitor = await comando.ExecuteReaderAsync();

        var jogos = new List<Jogo>();
        while (await leitor.ReadAsync())
        {
            jogos.Add(Mapear(leitor));
        }
        return (jogos, total);

        void AdicionarParametrosDoFiltro(MySqlCommand cmd)
        {
            AdicionarParametro(cmd, "@busca", busca);
            AdicionarParametro(cmd, "@idPlataforma", idPlataforma);
            AdicionarParametro(cmd, "@incluirInativos", incluirInativos);
            for (var i = 0; i < (ids?.Count ?? 0); i++)
            {
                AdicionarParametro(cmd, "@id" + i, ids![i]);
            }
        }
    }

    public async Task<Jogo?> BuscarPorIdAsync(int id)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT " + Colunas + " FROM tb_jogo WHERE co_seq_jogo = @id");
        AdicionarParametro(comando, "@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<int> InserirAsync(Jogo jogo)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "INSERT INTO tb_jogo (no_jogo, ds_jogo, nu_ano_lancamento, vl_preco, qt_estoque, " +
            "st_ativo, dt_cadastro, co_plataforma, co_condicao_jogo) " +
            "VALUES (@nome, @descricao, @anoLancamento, @preco, @estoque, " +
            "@ativo, @cadastro, @idPlataforma, @idCondicaoJogo)");
        AdicionarParametro(comando, "@nome", jogo.Nome);
        AdicionarParametro(comando, "@descricao", jogo.Descricao);
        AdicionarParametro(comando, "@anoLancamento", jogo.AnoLancamento);
        AdicionarParametro(comando, "@preco", jogo.Preco);
        AdicionarParametro(comando, "@estoque", jogo.Estoque);
        AdicionarParametro(comando, "@ativo", jogo.Ativo);
        AdicionarParametro(comando, "@cadastro", jogo.Cadastro);
        AdicionarParametro(comando, "@idPlataforma", jogo.IdPlataforma);
        AdicionarParametro(comando, "@idCondicaoJogo", jogo.IdCondicaoJogo);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    public async Task AlterarAsync(Jogo jogo)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "UPDATE tb_jogo SET no_jogo = @nome, ds_jogo = @descricao, " +
            "nu_ano_lancamento = @anoLancamento, vl_preco = @preco, qt_estoque = @estoque, " +
            "co_plataforma = @idPlataforma, co_condicao_jogo = @idCondicaoJogo " +
            "WHERE co_seq_jogo = @id");
        AdicionarParametro(comando, "@nome", jogo.Nome);
        AdicionarParametro(comando, "@descricao", jogo.Descricao);
        AdicionarParametro(comando, "@anoLancamento", jogo.AnoLancamento);
        AdicionarParametro(comando, "@preco", jogo.Preco);
        AdicionarParametro(comando, "@estoque", jogo.Estoque);
        AdicionarParametro(comando, "@idPlataforma", jogo.IdPlataforma);
        AdicionarParametro(comando, "@idCondicaoJogo", jogo.IdCondicaoJogo);
        AdicionarParametro(comando, "@id", jogo.Id);

        await ExecutarGravacaoAsync(comando);
    }

    public async Task DesativarAsync(int id)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "UPDATE tb_jogo SET st_ativo = FALSE WHERE co_seq_jogo = @id");
        AdicionarParametro(comando, "@id", id);

        await ExecutarGravacaoAsync(comando);
    }

    /// <summary>Monta "@id0, @id1, ..." com a quantidade informada. Só nomes, nunca valores.</summary>
    private static string NomesDeParametros(string prefixo, int quantidade)
    {
        return string.Join(", ", Enumerable.Range(0, quantidade).Select(i => prefixo + i));
    }

    /// <summary>Converte a linha atual do leitor num Jogo, coluna por coluna.</summary>
    private static Jogo Mapear(MySqlDataReader leitor)
    {
        return new Jogo
        {
            Id = leitor.GetInt32("co_seq_jogo"),
            Nome = leitor.GetString("no_jogo"),
            Descricao = leitor.IsDBNull(leitor.GetOrdinal("ds_jogo")) ? null : leitor.GetString("ds_jogo"),
            AnoLancamento = leitor.GetInt32("nu_ano_lancamento"),
            Preco = leitor.GetDecimal("vl_preco"),
            Estoque = leitor.GetInt32("qt_estoque"),
            Ativo = leitor.GetBoolean("st_ativo"),
            // dt_cadastro guarda data e hora em UTC.
            Cadastro = DateTime.SpecifyKind(leitor.GetDateTime("dt_cadastro"), DateTimeKind.Utc),
            IdPlataforma = leitor.GetInt32("co_plataforma"),
            IdCondicaoJogo = leitor.GetInt32("co_condicao_jogo"),
        };
    }
}
