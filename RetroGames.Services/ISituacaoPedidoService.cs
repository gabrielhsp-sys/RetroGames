using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Acesso às situações de pedido (somente leitura, sem regra de negócio).</summary>
public interface ISituacaoPedidoService
{
    /// <summary>Todas as situações de pedido, como o Repository entrega.</summary>
    Task<List<SituacaoPedido>> ListarAsync();
}
