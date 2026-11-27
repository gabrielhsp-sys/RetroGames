namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Filtro da gestão de pedidos, lido da query string de GET /api/Pedido (ex.: ?idSituacao=2).
/// Sem idSituacao, vêm todos os pedidos. O nome idSituacao é o do desafio.
/// </summary>
public class ListarPedidoRequestApiModel
{
    /// <summary>Só pedidos nesta situação (co_situacao_pedido).</summary>
    public int? IdSituacao { get; set; }
}
