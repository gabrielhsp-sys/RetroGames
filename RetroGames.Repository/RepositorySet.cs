namespace RetroGames.Repository;

/// <summary>Reúne os repositórios, todos com a mesma RepositoryConfig.</summary>
public class RepositorySet(RepositoryConfig config) : IRepositorySet
{
    public IPlataformaRepository Plataforma { get; } = new PlataformaRepository(config);
    public IJogoRepository Jogo { get; } = new JogoRepository(config);
    public IJogoGeneroRepository JogoGenero { get; } = new JogoGeneroRepository(config);
    public IGeneroRepository Genero { get; } = new GeneroRepository(config);
    public ICondicaoJogoRepository CondicaoJogo { get; } = new CondicaoJogoRepository(config);
    public IUsuarioRepository Usuario { get; } = new UsuarioRepository(config);
    public IPedidoRepository Pedido { get; } = new PedidoRepository(config);
    public IItemPedidoRepository ItemPedido { get; } = new ItemPedidoRepository(config);
    public ISituacaoPedidoRepository SituacaoPedido { get; } = new SituacaoPedidoRepository(config);
}
