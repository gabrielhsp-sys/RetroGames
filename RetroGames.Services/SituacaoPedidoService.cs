using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Service de situações de pedido (C07). Não tem regra de negócio, mas existe porque os
/// controllers falam só com Services, nunca direto com o Repository.
/// </summary>
public class SituacaoPedidoService(IRepositorySet repositorio) : ISituacaoPedidoService
{
    public async Task<List<SituacaoPedido>> ListarAsync()
    {
        return await repositorio.SituacaoPedido.ListarAsync();
    }
}
