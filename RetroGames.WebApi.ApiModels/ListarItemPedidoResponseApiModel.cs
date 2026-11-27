namespace RetroGames.WebApi.ApiModels;

/// <summary>Um item de pedido: o jogo, a quantidade e o preço pago (valor unitário no momento da compra).</summary>
public class ListarItemPedidoResponseApiModel
{
    public int IdJogo { get; set; }
    public string NomeJogo { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}
