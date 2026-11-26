using Moq;
using RetroGames.Model.Filtros;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Testes do JogoService. Cada repositório que o Service usa é um mock, e o
/// IRepositorySet simulado devolve todos eles. O usuário atual também é um mock
/// (por padrão, não é administrador). Nenhum teste usa o banco.
/// </summary>
public class JogoServiceTests
{
    private readonly Mock<IJogoRepository> jogoRepository = new();
    private readonly Mock<IPlataformaRepository> plataformaRepository = new();
    private readonly Mock<ICondicaoJogoRepository> condicaoJogoRepository = new();
    private readonly Mock<IGeneroRepository> generoRepository = new();
    private readonly Mock<IJogoGeneroRepository> jogoGeneroRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly Mock<IUsuarioAtualService> usuarioAtual = new();

    public JogoServiceTests()
    {
        repositorio.Setup(r => r.Jogo).Returns(jogoRepository.Object);
        repositorio.Setup(r => r.Plataforma).Returns(plataformaRepository.Object);
        repositorio.Setup(r => r.CondicaoJogo).Returns(condicaoJogoRepository.Object);
        repositorio.Setup(r => r.Genero).Returns(generoRepository.Object);
        repositorio.Setup(r => r.JogoGenero).Returns(jogoGeneroRepository.Object);
    }

    private JogoService CriarService()
    {
        return new JogoService(repositorio.Object, usuarioAtual.Object);
    }

    /// <summary>Um jogo da plataforma 1 e da condição 2, com valores válidos.</summary>
    private static Jogo CriarJogo()
    {
        return new Jogo
        {
            Nome = "Chrono Trigger",
            AnoLancamento = 1995,
            Preco = 199.90m,
            Estoque = 3,
            IdPlataforma = 1,
            IdCondicaoJogo = 2,
        };
    }

    [Fact]
    public async Task Inserir_PlataformaInexistente_LancaPlataformaNaoEncontradaENaoGrava()
    {
        // Arrange
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync((Plataforma?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(CriarJogo(), new List<int> { 1 }));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA, excecao.Erro);
        jogoRepository.Verify(r => r.InserirAsync(It.IsAny<Jogo>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_CondicaoInexistente_LancaCondicaoNaoEncontradaENaoGrava()
    {
        // Arrange: o jogo pede a condição 2, mas só existe a condição 1.
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(new Plataforma { Id = 1, Nome = "Super Nintendo" });
        condicaoJogoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<CondicaoJogo>
        {
            new() { Id = 1, Descricao = "Novo" },
        });
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(CriarJogo(), new List<int> { 1 }));

        // Assert
        Assert.Equal(BusinessErrors.CONDICAO_NAO_ENCONTRADA, excecao.Erro);
        jogoRepository.Verify(r => r.InserirAsync(It.IsAny<Jogo>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_GeneroInexistente_LancaGeneroNaoEncontradoENaoGrava()
    {
        // Arrange: o gênero 1 existe, o gênero 9 não.
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(1)).ReturnsAsync(new Plataforma { Id = 1, Nome = "Super Nintendo" });
        condicaoJogoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<CondicaoJogo>
        {
            new() { Id = 2, Descricao = "Usado" },
        });
        generoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<Genero>
        {
            new() { Id = 1, Nome = "RPG" },
        });
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.InserirAsync(CriarJogo(), new List<int> { 1, 9 }));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NAO_ENCONTRADO, excecao.Erro);
        jogoRepository.Verify(r => r.InserirAsync(It.IsAny<Jogo>()), Times.Never);
    }

    [Fact]
    public async Task Listar_FiltrandoPorGenero_EnviaSoIdsDosJogosDoGenero()
    {
        // Arrange: os jogos 10 e 12 são do gênero 1; o jogo 11 é do gênero 2.
        jogoGeneroRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<JogoGenero>
        {
            new() { IdJogo = 10, IdGenero = 1 },
            new() { IdJogo = 11, IdGenero = 2 },
            new() { IdJogo = 12, IdGenero = 1 },
        });
        jogoRepository.Setup(r => r.ListarAsync(It.IsAny<FiltroJogo>())).ReturnsAsync((new List<Jogo>(), 0));
        var service = CriarService();

        // Act
        await service.ListarAsync(new FiltroJogo { IdGenero = 1 });

        // Assert
        jogoRepository.Verify(r => r.ListarAsync(It.Is<FiltroJogo>(
            filtro => filtro.Ids != null && filtro.Ids.OrderBy(id => id).SequenceEqual(new[] { 10, 12 }))),
            Times.Once);
    }

    [Fact]
    public async Task Listar_SemGenero_NaoConsultaVinculosENaoRestringeIds()
    {
        // Arrange
        jogoRepository.Setup(r => r.ListarAsync(It.IsAny<FiltroJogo>())).ReturnsAsync((new List<Jogo>(), 0));
        var service = CriarService();

        // Act
        await service.ListarAsync(new FiltroJogo { IdGenero = null });

        // Assert
        jogoGeneroRepository.Verify(r => r.ListarAsync(), Times.Never);
        jogoRepository.Verify(r => r.ListarAsync(It.Is<FiltroJogo>(filtro => filtro.Ids == null)), Times.Once);
    }

    [Fact]
    public async Task Listar_PaginaComJogos_MontaNomePlataformaEDescricaoCondicao()
    {
        // Arrange: dois jogos, cada um com plataforma e condição diferentes.
        var jogos = new List<Jogo>
        {
            new() { Id = 10, Nome = "Chrono Trigger", IdPlataforma = 1, IdCondicaoJogo = 2, Ativo = true },
            new() { Id = 11, Nome = "Sonic", IdPlataforma = 2, IdCondicaoJogo = 1, Ativo = true },
        };
        jogoRepository.Setup(r => r.ListarAsync(It.IsAny<FiltroJogo>())).ReturnsAsync((jogos, 2));
        plataformaRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<Plataforma>
        {
            new() { Id = 1, Nome = "Super Nintendo" },
            new() { Id = 2, Nome = "Mega Drive" },
        });
        condicaoJogoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<CondicaoJogo>
        {
            new() { Id = 1, Descricao = "Novo" },
            new() { Id = 2, Descricao = "Usado" },
        });
        generoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<Genero>());
        jogoGeneroRepository.Setup(r => r.ListarPorJogosAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<JogoGenero>());
        var service = CriarService();

        // Act
        var (itens, _) = await service.ListarAsync(new FiltroJogo());

        // Assert
        var chrono = itens.Single(item => item.Jogo.Id == 10);
        var sonic = itens.Single(item => item.Jogo.Id == 11);
        Assert.Equal("Super Nintendo", chrono.NomePlataforma);
        Assert.Equal("Usado", chrono.DescricaoCondicao);
        Assert.Equal("Mega Drive", sonic.NomePlataforma);
        Assert.Equal("Novo", sonic.DescricaoCondicao);
    }

    [Fact]
    public async Task BuscarPorId_IdInexistente_LancaJogoNaoEncontrado()
    {
        // Arrange
        jogoRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Jogo?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarPorIdAsync(99));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_NAO_ENCONTRADO, excecao.Erro);
    }

    [Fact]
    public async Task Desativar_IdInexistente_LancaJogoNaoEncontradoENaoDesativa()
    {
        // Arrange
        jogoRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Jogo?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DesativarAsync(99));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_NAO_ENCONTRADO, excecao.Erro);
        jogoRepository.Verify(r => r.DesativarAsync(It.IsAny<int>()), Times.Never);
    }
}
