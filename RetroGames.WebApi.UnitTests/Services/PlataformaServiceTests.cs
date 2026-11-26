using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Testes das regras do PlataformaService. O Repository é um mock (Moq):
/// nenhum teste usa o banco. O xUnit cria uma instância nova desta classe
/// para cada teste, então cada teste começa com mocks novos.
/// </summary>
public class PlataformaServiceTests
{
    private readonly Mock<IPlataformaRepository> plataformaRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();

    public PlataformaServiceTests()
    {
        repositorio.Setup(r => r.Plataforma).Returns(plataformaRepository.Object);
    }

    private PlataformaService CriarService()
    {
        return new PlataformaService(repositorio.Object);
    }

    [Fact]
    public async Task BuscarPorId_IdInexistente_LancaNaoEncontrada()
    {
        // Arrange
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(99)).ReturnsAsync((Plataforma?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.BuscarPorIdAsync(99));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA, excecao.Erro);
    }

    [Fact]
    public async Task Inserir_NomeJaCadastrado_LancaNomeDuplicadoENaoInsere()
    {
        // Arrange
        var plataforma = new Plataforma { Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", null)).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.InserirAsync(plataforma));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, excecao.Erro);
        plataformaRepository.Verify(r => r.InserirAsync(It.IsAny<Plataforma>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_RepositoryLancaRegistroDuplicado_LancaNomeDuplicado()
    {
        // Arrange: o nome estava livre na conferência, mas outra gravação chegou antes ao banco.
        var plataforma = new Plataforma { Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", null)).ReturnsAsync(false);
        plataformaRepository.Setup(r => r.InserirAsync(plataforma))
            .ThrowsAsync(new RegistroDuplicadoException("Registro duplicado.", new Exception()));
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.InserirAsync(plataforma));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, excecao.Erro);
    }

    [Fact]
    public async Task Inserir_NomeLivre_DevolvePlataformaComIdGerado()
    {
        // Arrange
        var plataforma = new Plataforma { Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", null)).ReturnsAsync(false);
        plataformaRepository.Setup(r => r.InserirAsync(plataforma)).ReturnsAsync(7);
        var service = CriarService();

        // Act
        var resultado = await service.InserirAsync(plataforma);

        // Assert
        Assert.Equal(7, resultado.Id);
    }

    [Fact]
    public async Task Alterar_MantendoOProprioNome_GravaAlteracao()
    {
        // Arrange: o nome existe, mas é da própria plataforma 3. Só a consulta que
        // ignora o id 3 responde "não existe"; a que não ignora responderia "existe".
        var plataforma = new Plataforma { Id = 3, Nome = "Super Nintendo", Fabricante = "Nintendo", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(3)).ReturnsAsync(new Plataforma { Id = 3, Nome = "Super Nintendo" });
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", null)).ReturnsAsync(true);
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Super Nintendo", 3)).ReturnsAsync(false);
        var service = CriarService();

        // Act
        await service.AlterarAsync(plataforma);

        // Assert
        plataformaRepository.Verify(r => r.AlterarAsync(plataforma), Times.Once);
    }

    [Fact]
    public async Task Alterar_NomeDeOutraPlataforma_LancaNomeDuplicadoENaoGrava()
    {
        // Arrange: a plataforma 3 tenta usar um nome que já é de outra plataforma.
        var plataforma = new Plataforma { Id = 3, Nome = "Mega Drive", Fabricante = "Nintendo", AnoLancamento = 1990 };
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(3)).ReturnsAsync(new Plataforma { Id = 3, Nome = "Super Nintendo" });
        plataformaRepository.Setup(r => r.ExisteComNomeAsync("Mega Drive", 3)).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.AlterarAsync(plataforma));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_NOME_DUPLICADO, excecao.Erro);
        plataformaRepository.Verify(r => r.AlterarAsync(It.IsAny<Plataforma>()), Times.Never);
    }

    [Fact]
    public async Task Excluir_PlataformaComJogos_LancaPossuiJogosENaoExclui()
    {
        // Arrange
        plataformaRepository.Setup(r => r.BuscarPorIdAsync(3)).ReturnsAsync(new Plataforma { Id = 3, Nome = "Super Nintendo" });
        plataformaRepository.Setup(r => r.PossuiJogosAsync(3)).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.ExcluirAsync(3));

        // Assert
        Assert.Equal(BusinessErrors.PLATAFORMA_POSSUI_JOGOS, excecao.Erro);
        plataformaRepository.Verify(r => r.ExcluirAsync(It.IsAny<int>()), Times.Never);
    }
}
