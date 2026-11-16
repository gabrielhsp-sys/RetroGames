using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Acesso às condições de jogo (somente leitura, sem regra de negócio).</summary>
public interface ICondicaoJogoService
{
    /// <summary>Todas as condições de jogo, como o Repository entrega.</summary>
    Task<List<CondicaoJogo>> ListarAsync();
}
