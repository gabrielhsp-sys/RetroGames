using RetroGames.Model.Filtros;
using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Regras de negócio de pedidos do Módulo de Compras (C04 a C07).</summary>
public interface IPedidoService
{
    /// <summary>
    /// C04: finaliza a compra do usuário logado. Cada item traz só o jogo e a quantidade;
    /// o preço vem do banco. Jogo inexistente ou inativo: JOGO_INDISPONIVEL (400, código 110).
    /// Estoque insuficiente: ESTOQUE_INSUFICIENTE (409, código 111), sem gravar nada.
    /// Pedido, itens e baixa de estoque numa só transação. Devolve o pedido criado.
    /// </summary>
    Task<PedidoDetalheServiceModel> CriarAsync(List<ItemPedido> itens);

    /// <summary>
    /// C05: só os pedidos do usuário logado, do mais recente para o mais antigo,
    /// com a situação, o total e os itens (jogo, quantidade e preço pago).
    /// </summary>
    Task<List<PedidoDetalheServiceModel>> ListarMeusAsync();

    /// <summary>
    /// C06: o detalhe de um pedido, com a situação, a data, os itens e o total.
    /// Pedido inexistente: PEDIDO_NAO_ENCONTRADO (404, código 109). O cliente só vê
    /// os próprios pedidos: o de outro cliente dá PEDIDO_DE_OUTRO_USUARIO (403, código 113).
    /// O administrador vê qualquer pedido.
    /// </summary>
    Task<PedidoDetalheServiceModel> ObterAsync(int id);

    /// <summary>
    /// C07: todos os pedidos (ou só os de uma situação), do mais recente para o mais antigo,
    /// com o nome do cliente. Situação do filtro inexistente: SITUACAO_NAO_ENCONTRADA (404, código 112).
    /// </summary>
    Task<List<PedidoDetalheServiceModel>> ListarAsync(FiltroPedido filtro);

    /// <summary>
    /// C07: muda a situação do pedido e devolve o pedido atualizado. Pedido inexistente:
    /// PEDIDO_NAO_ENCONTRADO (404, código 109), conferido primeiro. Situação inexistente:
    /// SITUACAO_NAO_ENCONTRADA (404, código 112). Não há regra de transição entre situações.
    /// </summary>
    Task<PedidoDetalheServiceModel> AlterarSituacaoAsync(int id, int idSituacaoPedido);
}
