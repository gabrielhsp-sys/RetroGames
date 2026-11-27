namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de PUT /api/Pedido/{id}/situacao. O id do pedido vem da rota, nunca do corpo.</summary>
public class AlterarSituacaoPedidoRequestApiModel
{
    /// <summary>A nova situação (co_situacao_pedido): 1 Pendente, 2 Pago, 3 Enviado ou 4 Cancelado.</summary>
    public int IdSituacao { get; set; }
}
