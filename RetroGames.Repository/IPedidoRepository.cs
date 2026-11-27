using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_pedido.</summary>
public interface IPedidoRepository
{
    /// <summary>Grava um novo pedido e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Pedido pedido);
}
