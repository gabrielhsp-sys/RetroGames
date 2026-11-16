using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Service de condições de jogo. Não tem regra de negócio, mas existe porque os
/// controllers falam só com Services, nunca direto com o Repository.
/// </summary>
public class CondicaoJogoService(IRepositorySet repositorio) : ICondicaoJogoService
{
    public async Task<List<CondicaoJogo>> ListarAsync()
    {
        return await repositorio.CondicaoJogo.ListarAsync();
    }
}
