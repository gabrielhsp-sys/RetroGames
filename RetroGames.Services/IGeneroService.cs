using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>Regras de negócio de gêneros (C08), as mesmas da manutenção de plataformas.</summary>
public interface IGeneroService
{
    /// <summary>Todos os gêneros, como o Repository entrega.</summary>
    Task<List<Genero>> ListarAsync();

    /// <summary>O gênero do id. Se não existir, GENERO_NAO_ENCONTRADO.</summary>
    Task<Genero> BuscarPorIdAsync(int id);

    /// <summary>Grava o gênero (nome sem espaços nas pontas e único) e o devolve com o id gerado.</summary>
    Task<Genero> InserirAsync(Genero genero);

    /// <summary>Altera o gênero. Manter o próprio nome não é duplicidade.</summary>
    Task AlterarAsync(Genero genero);

    /// <summary>Exclui o gênero, se ele existir e não tiver jogos.</summary>
    Task ExcluirAsync(int id);
}
