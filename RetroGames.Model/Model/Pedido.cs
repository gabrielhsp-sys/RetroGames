namespace RetroGames.Model.Model;

/// <summary>Tabela tb_pedido.</summary>
public class Pedido
{
    /// <summary>Coluna co_seq_pedido.</summary>
    public int Id { get; set; }

    /// <summary>Coluna dt_pedido (data e hora em UTC).</summary>
    public DateTime Data { get; set; }

    /// <summary>Coluna vl_total (soma de quantidade × valor unitário dos itens).</summary>
    public decimal Total { get; set; }

    /// <summary>Coluna co_usuario (chave estrangeira para tb_usuario).</summary>
    public int IdUsuario { get; set; }

    /// <summary>Coluna co_situacao_pedido (chave estrangeira para tb_situacao_pedido).</summary>
    public int IdSituacaoPedido { get; set; }
}
