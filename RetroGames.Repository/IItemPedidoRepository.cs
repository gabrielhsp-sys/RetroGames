using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_item_pedido.</summary>
public interface IItemPedidoRepository
{
    /// <summary>
    /// Grava os itens de um pedido num único INSERT. Jogo repetido no mesmo pedido:
    /// RegistroDuplicadoException (UNIQUE co_pedido, co_jogo).
    /// </summary>
    Task InserirAsync(int idPedido, List<ItemPedido> itens);
}
