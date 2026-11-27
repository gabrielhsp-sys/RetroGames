using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Regras de negócio de pedidos do Módulo de Compras (C04).</summary>
public interface IPedidoService
{
    /// <summary>
    /// C04: finaliza a compra do usuário logado. Cada item traz só o jogo e a quantidade;
    /// o preço vem do banco. Jogo inexistente ou inativo: JOGO_INDISPONIVEL (400, código 110).
    /// Estoque insuficiente: ESTOQUE_INSUFICIENTE (409, código 111), sem gravar nada.
    /// Pedido, itens e baixa de estoque numa só transação. Devolve o pedido criado.
    /// </summary>
    Task<PedidoDetalheServiceModel> CriarAsync(List<ItemPedido> itens);
}
