namespace RetroGames.WebApi.ApiModels;

/// <summary>Uma situação de pedido, como sai no JSON de GET /api/SituacaoPedido.</summary>
public class ListarSituacaoPedidoResponseApiModel
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
