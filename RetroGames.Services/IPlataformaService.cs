using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Regras de negócio de plataformas (RN01 e RN02).</summary>
public interface IPlataformaService
{
    /// <summary>Todas as plataformas, como o Repository entrega.</summary>
    Task<List<Plataforma>> ListarAsync();

    /// <summary>A plataforma do id. Se não existir, PLATAFORMA_NAO_ENCONTRADA.</summary>
    Task<Plataforma> BuscarPorIdAsync(int id);

    /// <summary>Grava a plataforma (nome sem espaços nas pontas e único) e a devolve com o id gerado.</summary>
    Task<Plataforma> InserirAsync(Plataforma plataforma);

    /// <summary>Altera a plataforma. Manter o próprio nome não é duplicidade.</summary>
    Task AlterarAsync(Plataforma plataforma);

    /// <summary>Exclui a plataforma, se ela existir e não tiver jogos.</summary>
    Task ExcluirAsync(int id);
}
