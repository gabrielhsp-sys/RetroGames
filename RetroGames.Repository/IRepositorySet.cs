namespace RetroGames.Repository;

/// <summary>Um único objeto que dá acesso a todos os repositórios. O Service o recebe no construtor.</summary>
public interface IRepositorySet
{
    IPlataformaRepository Plataforma { get; }
    IJogoRepository Jogo { get; }
    IJogoGeneroRepository JogoGenero { get; }
    IGeneroRepository Genero { get; }
    ICondicaoJogoRepository CondicaoJogo { get; }
}
