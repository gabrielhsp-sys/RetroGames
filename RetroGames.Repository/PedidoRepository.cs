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
}
