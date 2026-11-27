using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_pedido. Só executa SQL e devolve dados.</summary>
public class PedidoRepository(RepositoryConfig config) : RepositoryBase(config), IPedidoRepository
{
    public async Task<int> InserirAsync(Pedido pedido)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "INSERT INTO tb_pedido (dt_pedido, vl_total, co_usuario, co_situacao_pedido) " +
            "VALUES (@data, @total, @idUsuario, @idSituacaoPedido)");
        AdicionarParametro(comando, "@data", pedido.Data);
        AdicionarParametro(comando, "@total", pedido.Total);
        AdicionarParametro(comando, "@idUsuario", pedido.IdUsuario);
        AdicionarParametro(comando, "@idSituacaoPedido", pedido.IdSituacaoPedido);

        await ExecutarGravacaoAsync(comando);
        return (int)comando.LastInsertedId;
    }

    public async Task<List<Pedido>> ListarPorUsuarioAsync(int idUsuario)
    {
        await using var conexao = await CriarConexaoAsync();
        // Empate na data (mesmo segundo): o id maior é o pedido mais recente.
        await using var comando = CriarComando(conexao,
            "SELECT co_seq_pedido, dt_pedido, vl_total, co_usuario, co_situacao_pedido FROM tb_pedido " +
            "WHERE co_usuario = @idUsuario ORDER BY dt_pedido DESC, co_seq_pedido DESC");
        AdicionarParametro(comando, "@idUsuario", idUsuario);
        await using var leitor = await comando.ExecuteReaderAsync();

        var pedidos = new List<Pedido>();
        while (await leitor.ReadAsync())
        {
            pedidos.Add(Mapear(leitor));
        }
        return pedidos;
    }

    public async Task<Pedido?> BuscarPorIdAsync(int id)
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT co_seq_pedido, dt_pedido, vl_total, co_usuario, co_situacao_pedido FROM tb_pedido " +
            "WHERE co_seq_pedido = @id");
        AdicionarParametro(comando, "@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    /// <summary>Converte a linha atual do leitor num Pedido, coluna por coluna.</summary>
    private static Pedido Mapear(MySqlDataReader leitor)
    {
        return new Pedido
        {
            Id = leitor.GetInt32("co_seq_pedido"),
            // dt_pedido guarda data e hora em UTC.
            Data = DateTime.SpecifyKind(leitor.GetDateTime("dt_pedido"), DateTimeKind.Utc),
            Total = leitor.GetDecimal("vl_total"),
            IdUsuario = leitor.GetInt32("co_usuario"),
            IdSituacaoPedido = leitor.GetInt32("co_situacao_pedido"),
        };
    }
}
