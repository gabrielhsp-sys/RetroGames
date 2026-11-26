using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Teste do GeneroService. Ele só lê dados, mas também tem teste: assim todo
/// Service do catálogo (plataformas, jogos, gêneros e condições) tem teste.
/// O Repository é um mock.
/// </summary>
public class GeneroServiceTests
{
    private readonly Mock<IGeneroRepository> generoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();

    public GeneroServiceTests()
    {
        repositorio.Setup(r => r.Genero).Returns(generoRepository.Object);
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
}
