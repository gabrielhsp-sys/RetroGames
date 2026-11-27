using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Testes do GeneroService (C08): a leitura e as regras da manutenção, as
/// mesmas da manutenção de plataformas. Os Repositories são mocks (Moq):
/// nenhum teste usa o banco. O xUnit cria uma instância nova desta classe
/// para cada teste, então cada teste começa com mocks novos.
/// </summary>
public class GeneroServiceTests
{
    private readonly Mock<IGeneroRepository> generoRepository = new();
    private readonly Mock<IJogoGeneroRepository> jogoGeneroRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();

    public GeneroServiceTests()
    {
        repositorio.Setup(r => r.Genero).Returns(generoRepository.Object);
        repositorio.Setup(r => r.JogoGenero).Returns(jogoGeneroRepository.Object);
    }

    private GeneroService CriarService()
    {
        return new GeneroService(repositorio.Object);
    }

    [Fact]
    public async Task Listar_RepositoryComGeneros_DevolveOsGenerosDoRepository()
    {
        // Arrange
        var generos = new List<Genero>
        {
            new() { Id = 1, Nome = "Plataforma" },
            new() { Id = 2, Nome = "RPG" },
        };
        generoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(generos);
        var service = CriarService();

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Equal(generos, resultado);
    }

    [Fact]
    public async Task BuscarPorId_IdInexistente_LancaNaoEncontrado()
    {
        // Arrange
        generoRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Genero?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarPorIdAsync(99));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NAO_ENCONTRADO, excecao.Erro);
    }

    [Fact]
    public async Task Inserir_NomeLivre_DevolveGeneroComIdGerado()
    {
        // Arrange
        var genero = new Genero { Nome = "Corrida" };
        generoRepository.Setup(r => r.ExisteComNomeAsync("Corrida", null)).ReturnsAsync(false);
        generoRepository.Setup(r => r.InserirAsync(genero)).ReturnsAsync(9);
        var service = CriarService();

        // Act
        var resultado = await service.InserirAsync(genero);

        // Assert
        Assert.Equal(9, resultado.Id);
        generoRepository.Verify(r => r.InserirAsync(genero), Times.Once);
    }

    [Fact]
    public async Task Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoInsere()
    {
        // Arrange
        var genero = new Genero { Nome = "RPG" };
        generoRepository.Setup(r => r.ExisteComNomeAsync("RPG", null)).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.InserirAsync(genero));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NOME_DUPLICADO, excecao.Erro);
        generoRepository.Verify(r => r.InserirAsync(It.IsAny<Genero>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_RepositoryLancaRegistroDuplicado_LancaNomeDuplicado()
    {
        // Arrange: o nome estava livre na conferência, mas outra gravação chegou antes ao banco.
        var genero = new Genero { Nome = "Corrida" };
        generoRepository.Setup(r => r.ExisteComNomeAsync("Corrida", null)).ReturnsAsync(false);
        generoRepository.Setup(r => r.InserirAsync(genero))
            .ThrowsAsync(new RegistroDuplicadoException("Registro duplicado.", new Exception()));
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.InserirAsync(genero));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NOME_DUPLICADO, excecao.Erro);
    }

    [Fact]
    public async Task Alterar_MantendoOProprioNome_GravaAlteracao()
    {
        // Arrange: o nome existe, mas é do próprio gênero 2. Só a consulta que
        // ignora o id 2 responde "não existe"; a que não ignora responderia "existe".
        var genero = new Genero { Id = 2, Nome = "RPG" };
        generoRepository.Setup(r => r.BuscarPorIdAsync(2)).ReturnsAsync(new Genero { Id = 2, Nome = "RPG" });
        generoRepository.Setup(r => r.ExisteComNomeAsync("RPG", null)).ReturnsAsync(true);
        generoRepository.Setup(r => r.ExisteComNomeAsync("RPG", 2)).ReturnsAsync(false);
        var service = CriarService();

        // Act
        await service.AlterarAsync(genero);

        // Assert
        generoRepository.Verify(r => r.AlterarAsync(genero), Times.Once);
    }

    [Fact]
    public async Task Alterar_NomeDeOutroGenero_LancaNomeDuplicadoENaoGrava()
    {
        // Arrange: o gênero 2 tenta usar um nome que já é de outro gênero.
        var genero = new Genero { Id = 2, Nome = "Plataforma" };
        generoRepository.Setup(r => r.BuscarPorIdAsync(2)).ReturnsAsync(new Genero { Id = 2, Nome = "RPG" });
        generoRepository.Setup(r => r.ExisteComNomeAsync("Plataforma", 2)).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.AlterarAsync(genero));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NOME_DUPLICADO, excecao.Erro);
        generoRepository.Verify(r => r.AlterarAsync(It.IsAny<Genero>()), Times.Never);
    }

    [Fact]
    public async Task Alterar_IdInexistente_LancaNaoEncontradoENaoGrava()
    {
        // Arrange
        var genero = new Genero { Id = 99, Nome = "Corrida" };
        generoRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Genero?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.AlterarAsync(genero));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NAO_ENCONTRADO, excecao.Erro);
        generoRepository.Verify(r => r.AlterarAsync(It.IsAny<Genero>()), Times.Never);
    }

    [Fact]
    public async Task Excluir_GeneroComJogos_LancaPossuiJogosENaoExclui()
    {
        // Arrange
        generoRepository.Setup(r => r.BuscarPorIdAsync(2)).ReturnsAsync(new Genero { Id = 2, Nome = "RPG" });
        jogoGeneroRepository.Setup(r => r.ExisteComGeneroAsync(2)).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.ExcluirAsync(2));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_POSSUI_JOGOS, excecao.Erro);
        generoRepository.Verify(r => r.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Excluir_GeneroSemJogos_ExcluiOGenero()
    {
        // Arrange
        generoRepository.Setup(r => r.BuscarPorIdAsync(2)).ReturnsAsync(new Genero { Id = 2, Nome = "RPG" });
        jogoGeneroRepository.Setup(r => r.ExisteComGeneroAsync(2)).ReturnsAsync(false);
        var service = CriarService();

        // Act
        await service.ExcluirAsync(2);

        // Assert
        generoRepository.Verify(r => r.ExcluirAsync(2), Times.Once);
    }

    [Fact]
    public async Task Excluir_IdInexistente_LancaNaoEncontradoENaoExclui()
    {
        // Arrange
        generoRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Genero?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.ExcluirAsync(99));

        // Assert
        Assert.Equal(BusinessErrors.GENERO_NAO_ENCONTRADO, excecao.Erro);
        generoRepository.Verify(r => r.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }
}
