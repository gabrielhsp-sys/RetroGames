namespace RetroGames.WebApi.ApiModels;

/// <summary>Um item do corpo de POST /api/Pedido: o jogo e a quantidade (1 a 10).</summary>
public class InserirItemPedidoRequestApiModel
{
    public int IdJogo { get; set; }
    public int Quantidade { get; set; }
}
