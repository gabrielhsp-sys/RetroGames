using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Service de gêneros. Não tem regra de negócio, mas existe porque os
/// controllers falam só com Services, nunca direto com o Repository.
/// </summary>
public class GeneroService(IRepositorySet repositorio) : IGeneroService
{
    public async Task<List<Genero>> ListarAsync()
    {
        return await repositorio.Genero.ListarAsync();
    }
}
