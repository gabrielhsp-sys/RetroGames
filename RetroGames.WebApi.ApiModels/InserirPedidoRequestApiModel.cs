namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Corpo de POST /api/Pedido (C04): os itens do carrinho. Não há preço nem total:
/// o preço vem do banco e o total é calculado no Service. Campo a mais no JSON é ignorado.
/// </summary>
public class InserirPedidoRequestApiModel
{
    /// <summary>Ao menos um item; cada jogo uma só vez.</summary>
    public List<InserirItemPedidoRequestApiModel> Itens { get; set; } = new();
}
