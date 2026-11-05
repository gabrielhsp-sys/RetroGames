namespace RetroGames.Repository;

/// <summary>Reúne os repositórios, todos com a mesma RepositoryConfig.</summary>
public class RepositorySet(RepositoryConfig config) : IRepositorySet
{
    public IPlataformaRepository Plataforma { get; } = new PlataformaRepository(config);
    public IJogoRepository Jogo { get; } = new JogoRepository(config);
    public IJogoGeneroRepository JogoGenero { get; } = new JogoGeneroRepository(config);
    public IGeneroRepository Genero { get; } = new GeneroRepository(config);
    public ICondicaoJogoRepository CondicaoJogo { get; } = new CondicaoJogoRepository(config);
}
