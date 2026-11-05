using MySqlConnector;

namespace RetroGames.Repository;

/// <summary>
/// Base de todos os repositórios: conexão, comando, parâmetros e gravação.
/// Não tem SQL de nenhuma tabela. Quem chama fecha conexão, comando e leitor
/// com await using, o que vale mesmo quando ocorre erro.
/// </summary>
public abstract class RepositoryBase(RepositoryConfig config)
{
    /// <summary>Cria e abre uma conexão com o banco.</summary>
    protected async Task<MySqlConnection> CriarConexaoAsync()
    {
        var conexao = new MySqlConnection(config.ConnectionString);
        try
        {
            await conexao.OpenAsync();
            return conexao;
        }
        catch
        {
            // A conexão não chegou a quem chamou: fecha aqui antes de repassar o erro.
            await conexao.DisposeAsync();
            throw;
        }
    }

    /// <summary>Cria um comando com o texto SQL na conexão informada.</summary>
    protected MySqlCommand CriarComando(MySqlConnection conexao, string sql)
    {
        return new MySqlCommand(sql, conexao);
    }

    /// <summary>Adiciona um parâmetro (@nome e valor) ao comando. Nulo vira NULL no banco.</summary>
    protected void AdicionarParametro(MySqlCommand comando, string nome, object? valor)
    {
        comando.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
    }

    /// <summary>
    /// Executa uma gravação (INSERT ou UPDATE) e devolve o número de linhas afetadas.
    /// Único lugar que converte o erro 1062 do MySQL em RegistroDuplicadoException.
    /// </summary>
    protected async Task<int> ExecutarGravacaoAsync(MySqlCommand comando)
    {
        try
        {
            return await comando.ExecuteNonQueryAsync();
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            throw new RegistroDuplicadoException(ex.Message, ex);
        }
    }
}
