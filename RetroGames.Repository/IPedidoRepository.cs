using RetroGames.Model.Filtros;
using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Contrato com todo o SQL da tabela tb_pedido.</summary>
public interface IPedidoRepository
{
    /// <summary>Grava um novo pedido e devolve o id gerado pelo banco.</summary>
    Task<int> InserirAsync(Pedido pedido);

    /// <summary>Os pedidos de um usuário, do mais recente para o mais antigo.</summary>
    Task<List<Pedido>> ListarPorUsuarioAsync(int idUsuario);

    /// <summary>O pedido com o id informado, ou nulo se não existir.</summary>
    Task<Pedido?> BuscarPorIdAsync(int id);

    /// <summary>Todos os pedidos (ou só os de uma situação), do mais recente para o mais antigo.</summary>
    Task<List<Pedido>> ListarAsync(FiltroPedido filtro);

    /// <summary>Troca a situação do pedido. Devolve se alguma linha foi alterada.</summary>
    Task<bool> AlterarSituacaoAsync(int id, int idSituacaoPedido);
}
