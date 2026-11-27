using MySqlConnector;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_item_pedido. Só executa SQL e devolve dados.</summary>
public class ItemPedidoRepository(RepositoryConfig config) : RepositoryBase(config), IItemPedidoRepository
{
    public async Task InserirAsync(int idPedido, List<ItemPedido> itens)
    {
        if (itens.Count == 0)
        {
            return;
        }

        await using var conexao = await CriarConexaoAsync();
        // Um único INSERT com uma linha (@quantidadeN, @valorUnitarioN, @idPedido, @idJogoN) por item.
        var linhas = string.Join(", ", Enumerable.Range(0, itens.Count)
            .Select(i => "(@quantidade" + i + ", @valorUnitario" + i + ", @idPedido, @idJogo" + i + ")"));
        await using var comando = CriarComando(conexao,
            "INSERT INTO tb_item_pedido (qt_item, vl_unitario, co_pedido, co_jogo) VALUES " + linhas);
        AdicionarParametro(comando, "@idPedido", idPedido);
        for (var i = 0; i < itens.Count; i++)
        {
            AdicionarParametro(comando, "@quantidade" + i, itens[i].Quantidade);
            AdicionarParametro(comando, "@valorUnitario" + i, itens[i].ValorUnitario);
            AdicionarParametro(comando, "@idJogo" + i, itens[i].IdJogo);
        }

        await ExecutarGravacaoAsync(comando);
    }

    public async Task<List<ItemPedido>> ListarPorPedidosAsync(List<int> idsPedido)
    {
        // Sem ids, não há item a buscar; "IN ()" seria SQL inválido.
        if (idsPedido.Count == 0)
        {
            return new List<ItemPedido>();
        }

        await using var conexao = await CriarConexaoAsync();
        // Só os nomes dos parâmetros (@id0, @id1...) entram no texto; os valores vão como parâmetro.
        var nomes = string.Join(", ", Enumerable.Range(0, idsPedido.Count).Select(i => "@id" + i));
        await using var comando = CriarComando(conexao,
            "SELECT co_seq_item_pedido, qt_item, vl_unitario, co_pedido, co_jogo FROM tb_item_pedido " +
            "WHERE co_pedido IN (" + nomes + ") ORDER BY co_pedido, co_seq_item_pedido");
        for (var i = 0; i < idsPedido.Count; i++)
        {
            AdicionarParametro(comando, "@id" + i, idsPedido[i]);
        }
        await using var leitor = await comando.ExecuteReaderAsync();

        var itens = new List<ItemPedido>();
        while (await leitor.ReadAsync())
        {
            itens.Add(Mapear(leitor));
        }
        return itens;
    }

    /// <summary>Converte a linha atual do leitor num ItemPedido, coluna por coluna.</summary>
    private static ItemPedido Mapear(MySqlDataReader leitor)
    {
        return new ItemPedido
        {
            Id = leitor.GetInt32("co_seq_item_pedido"),
            Quantidade = leitor.GetInt32("qt_item"),
            ValorUnitario = leitor.GetDecimal("vl_unitario"),
            IdPedido = leitor.GetInt32("co_pedido"),
            IdJogo = leitor.GetInt32("co_jogo"),
        };
    }
}
