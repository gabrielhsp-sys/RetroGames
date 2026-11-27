namespace RetroGames.Model.Model;

/// <summary>Tabela tb_situacao_pedido (domínio, chaves fixas em SituacaoPedidoIds).</summary>
public class SituacaoPedido
{
    /// <summary>Coluna co_situacao_pedido.</summary>
    public int Id { get; set; }

    /// <summary>Coluna ds_situacao_pedido.</summary>
    public string Descricao { get; set; } = string.Empty;
}
