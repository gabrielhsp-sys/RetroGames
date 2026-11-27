namespace RetroGames.Model.Model;

/// <summary>Tabela tb_item_pedido (um jogo aparece uma só vez em cada pedido).</summary>
public class ItemPedido
{
    /// <summary>Coluna co_seq_item_pedido.</summary>
    public int Id { get; set; }

    /// <summary>Coluna qt_item.</summary>
    public int Quantidade { get; set; }

    /// <summary>Coluna vl_unitario (preço do jogo no momento da compra).</summary>
    public decimal ValorUnitario { get; set; }

    /// <summary>Coluna co_pedido (chave estrangeira para tb_pedido).</summary>
    public int IdPedido { get; set; }

    /// <summary>Coluna co_jogo (chave estrangeira para tb_jogo).</summary>
    public int IdJogo { get; set; }
}
