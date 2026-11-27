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
}
