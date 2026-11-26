using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Teste do CondicaoJogoService. Ele só lê dados, mas também tem teste: assim
/// todo Service tem pelo menos um. O Repository é um mock.
/// </summary>
public class CondicaoJogoServiceTests
{
    private readonly Mock<ICondicaoJogoRepository> condicaoJogoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();

    public CondicaoJogoServiceTests()
    {
        repositorio.Setup(r => r.CondicaoJogo).Returns(condicaoJogoRepository.Object);
    }

    private CondicaoJogoService CriarService()
    {
        return new CondicaoJogoService(repositorio.Object);
    }

    [Fact]
    public async Task Listar_RepositoryComCondicoes_DevolveAsCondicoesDoRepository()
    {
        // Arrange
        var condicoes = new List<CondicaoJogo>
        {
            new() { Id = 1, Descricao = "Novo" },
            new() { Id = 2, Descricao = "Usado" },
        };
        condicaoJogoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(condicoes);
        var service = CriarService();

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Equal(condicoes, resultado);
    }
}
