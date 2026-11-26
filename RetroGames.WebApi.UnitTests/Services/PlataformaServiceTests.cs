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
}
