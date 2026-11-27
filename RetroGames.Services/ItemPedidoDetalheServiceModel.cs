using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Um item de pedido com o nome do jogo (tb_jogo), combinado com LINQ no PedidoService.</summary>
public class ItemPedidoDetalheServiceModel
{
    /// <summary>O item, como o Repository entrega (tb_item_pedido), com o preço pago.</summary>
    public ItemPedido Item { get; set; } = new();

    /// <summary>Nome do jogo do item (tb_jogo).</summary>
    public string NomeJogo { get; set; } = string.Empty;
}
