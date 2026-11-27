namespace RetroGames.WebApi.ApiModels;

/// <summary>Um pedido com a situação, o total e os itens (POST /api/Pedido e GET /api/Pedido/meus).</summary>
public class ListarPedidoResponseApiModel
{
    public int Id { get; set; }

    /// <summary>Data e hora do pedido, em UTC.</summary>
    public DateTime Data { get; set; }

    public decimal Total { get; set; }
    public int IdSituacaoPedido { get; set; }

    /// <summary>"Pendente", "Pago", "Enviado" ou "Cancelado".</summary>
    public string DescricaoSituacao { get; set; } = string.Empty;

    public List<ListarItemPedidoResponseApiModel> Itens { get; set; } = new();
}
