using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_situacao_pedido.</summary>
public interface ISituacaoPedidoRepository
{
    /// <summary>Todas as situações de pedido, pela chave.</summary>
    Task<List<SituacaoPedido>> ListarAsync();
}
