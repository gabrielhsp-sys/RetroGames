using Moq;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Teste do SituacaoPedidoService (C07). Ele só lê dados, mas também tem teste,
/// como o CondicaoJogoService. O Repository é um mock.
/// </summary>
public class SituacaoPedidoServiceTests
{
    private readonly Mock<ISituacaoPedidoRepository> situacaoPedidoRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();

    public SituacaoPedidoServiceTests()
    {
        repositorio.Setup(r => r.SituacaoPedido).Returns(situacaoPedidoRepository.Object);
    }

    private SituacaoPedidoService CriarService()
    {
        return new SituacaoPedidoService(repositorio.Object);
    }

    [Fact]
    public async Task Listar_RepositoryComSituacoes_DevolveAsSituacoesDoRepository()
    {
        // Arrange
        var situacoes = new List<SituacaoPedido>
        {
            new() { Id = 1, Descricao = "Pendente" },
            new() { Id = 2, Descricao = "Pago" },
            new() { Id = 3, Descricao = "Enviado" },
            new() { Id = 4, Descricao = "Cancelado" },
        };
        situacaoPedidoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(situacoes);
        var service = CriarService();

        // Act
        var resultado = await service.ListarAsync();

        // Assert
        Assert.Equal(situacoes, resultado);
    }
}
