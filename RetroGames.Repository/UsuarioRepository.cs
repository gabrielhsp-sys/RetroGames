using MySqlConnector;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_usuario. Só executa SQL e devolve dados; nunca vê a senha, só o hash.</summary>
public class UsuarioRepository(RepositoryConfig config) : RepositoryBase(config), IUsuarioRepository
{
    private const string Colunas =
        "co_seq_usuario, no_usuario, ds_email, ds_senha_hash, st_ativo, dt_cadastro, co_perfil";

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT " + Colunas + " FROM tb_usuario WHERE ds_email = @email");
        AdicionarParametro(comando, "@email", email);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT " + Colunas + " FROM tb_usuario WHERE co_seq_usuario = @id");
        AdicionarParametro(comando, "@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<List<Usuario>> ListarPorIdsAsync(List<int> ids)
    {
        // Sem ids, não há usuário a buscar; "IN ()" seria SQL inválido.
        if (ids.Count == 0)
        {
            return new List<Usuario>();
        }

        await using var conexao = await CriarConexaoAsync();
        // Só os nomes dos parâmetros (@id0, @id1...) entram no texto; os valores vão como parâmetro.
        var nomes = string.Join(", ", Enumerable.Range(0, ids.Count).Select(i => "@id" + i));
        await using var comando = CriarComando(conexao,
            "SELECT " + Colunas + " FROM tb_usuario WHERE co_seq_usuario IN (" + nomes + ")");
        for (var i = 0; i < ids.Count; i++)
        {
            AdicionarParametro(comando, "@id" + i, ids[i]);
        }
        await using var leitor = await comando.ExecuteReaderAsync();

        var usuarios = new List<Usuario>();
        while (await leitor.ReadAsync())
        {
            usuarios.Add(Mapear(leitor));
        }
        return usuarios;
    }

    public async Task<bool> ExisteAdministradorAsync()
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT COUNT(*) FROM tb_usuario WHERE co_perfil = @idPerfil");
        AdicionarParametro(comando, "@idPerfil", PerfilIds.Administrador);

        return Convert.ToInt64(await comando.ExecuteScalarAsync()) > 0;
    }

    public async Task<int> InserirAsync(Usuario usuario)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "INSERT INTO tb_usuario (no_usuario, ds_email, ds_senha_hash, st_ativo, dt_cadastro, co_perfil) " +
            "VALUES (@nome, @email, @senhaHash, @ativo, @cadastro, @idPerfil)");
        AdicionarParametro(comando, "@nome", usuario.Nome);
        AdicionarParametro(comando, "@email", usuario.Email);
        AdicionarParametro(comando, "@senhaHash", usuario.SenhaHash);
        AdicionarParametro(comando, "@ativo", usuario.Ativo);
        AdicionarParametro(comando, "@cadastro", usuario.Cadastro);
        AdicionarParametro(comando, "@idPerfil", usuario.IdPerfil);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    /// <summary>Converte a linha atual do leitor num Usuario, coluna por coluna.</summary>
    private static Usuario Mapear(MySqlDataReader leitor)
    {
        return new Usuario
        {
            Id = leitor.GetInt32("co_seq_usuario"),
            Nome = leitor.GetString("no_usuario"),
            Email = leitor.GetString("ds_email"),
            SenhaHash = leitor.GetString("ds_senha_hash"),
            Ativo = leitor.GetBoolean("st_ativo"),
            // dt_cadastro guarda data e hora em UTC.
            Cadastro = DateTime.SpecifyKind(leitor.GetDateTime("dt_cadastro"), DateTimeKind.Utc),
            IdPerfil = leitor.GetInt32("co_perfil"),
        };
    }
}
