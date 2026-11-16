using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Acesso aos gêneros (somente leitura, sem regra de negócio).</summary>
public interface IGeneroService
{
    /// <summary>Todos os gêneros, como o Repository entrega.</summary>
    Task<List<Genero>> ListarAsync();
}
