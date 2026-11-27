using Moq;
using RetroGames.Model.Constantes;
using RetroGames.Model.Filtros;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Testes do PedidoService (C04 a C07). Cada repositório que o Service usa é um mock,
/// e o IRepositorySet simulado devolve todos eles. O usuário logado é o 5.
/// Nenhum teste usa o banco.
/// </summary>
public class PedidoServiceTests
{
    private const int IdUsuarioLogado = 5;

    private readonly Mock<IJogoRepository> jogoRepository = new();
    private readonly Mock<IPedidoRepository> pedidoRepository = new();
    private readonly Mock<IItemPedidoRepository> itemPedidoRepository = new();
    private readonly Mock<ISituacaoPedidoRepository> situacaoPedidoRepository = new();
    private readonly Mock<IUsuarioRepository> usuarioRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly Mock<IUsuarioAtualService> usuarioAtual = new();

    public PedidoServiceTests()
    {
        repositorio.Setup(r => r.Jogo).Returns(jogoRepository.Object);
        repositorio.Setup(r => r.Pedido).Returns(pedidoRepository.Object);
        repositorio.Setup(r => r.ItemPedido).Returns(itemPedidoRepository.Object);
        repositorio.Setup(r => r.SituacaoPedido).Returns(situacaoPedidoRepository.Object);
        repositorio.Setup(r => r.Usuario).Returns(usuarioRepository.Object);
        usuarioAtual.Setup(u => u.Id).Returns(IdUsuarioLogado);
        situacaoPedidoRepository.Setup(r => r.ListarAsync()).ReturnsAsync(new List<SituacaoPedido>
        {
            new() { Id = SituacaoPedidoIds.Pendente, Descricao = "Pendente" },
            new() { Id = SituacaoPedidoIds.Pago, Descricao = "Pago" },
        });
        // Por padrão a baixa de estoque dá certo e o pedido gravado recebe o id 10.
        jogoRepository.Setup(r => r.BaixarEstoqueAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        pedidoRepository.Setup(r => r.InserirAsync(It.IsAny<Pedido>())).ReturnsAsync(10);
    }

    private PedidoService CriarService()
    {
        return new PedidoService(repositorio.Object, usuarioAtual.Object);
    }

    private static Jogo CriarJogo(int id, decimal preco, int estoque, bool ativo = true)
    {
        return new Jogo { Id = id, Nome = "Jogo " + id, Preco = preco, Estoque = estoque, Ativo = ativo };
    }

    /// <summary>O banco tem estes jogos: ListarPorIdsAsync devolve os que estão na lista pedida.</summary>
    private void JogosNoBanco(params Jogo[] jogos)
    {
        jogoRepository.Setup(r => r.ListarPorIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync((List<int> ids) => jogos.Where(jogo => ids.Contains(jogo.Id)).ToList());
    }

    private static ItemPedido Item(int idJogo, int quantidade)
    {
        return new ItemPedido { IdJogo = idJogo, Quantidade = quantidade };
    }

    /// <summary>Confere que nada foi gravado: nem baixa, nem pedido, nem itens.</summary>
    private void NadaFoiGravado()
    {
        jogoRepository.Verify(r => r.BaixarEstoqueAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        pedidoRepository.Verify(r => r.InserirAsync(It.IsAny<Pedido>()), Times.Never);
        itemPedidoRepository.Verify(r => r.InserirAsync(It.IsAny<int>(), It.IsAny<List<ItemPedido>>()), Times.Never);
    }

    [Fact]
    public async Task Criar_ItensValidos_GravaPedidoPendenteDoUsuarioComTotalCalculado()
    {
        // Arrange: 2 × 100,00 + 1 × 59,90 = 259,90.
        JogosNoBanco(CriarJogo(1, 100.00m, 5), CriarJogo(2, 59.90m, 5));
        Pedido? gravado = null;
        pedidoRepository.Setup(r => r.InserirAsync(It.IsAny<Pedido>()))
            .Callback<Pedido>(pedido => gravado = pedido)
            .ReturnsAsync(10);
        var service = CriarService();

        // Act
        await service.CriarAsync(new List<ItemPedido> { Item(1, 2), Item(2, 1) });

        // Assert
        Assert.NotNull(gravado);
        Assert.Equal(259.90m, gravado.Total);
        Assert.Equal(SituacaoPedidoIds.Pendente, gravado.IdSituacaoPedido);
        Assert.Equal(IdUsuarioLogado, gravado.IdUsuario);
        Assert.Equal(DateTimeKind.Utc, gravado.Data.Kind);
    }

    [Fact]
    public async Task Criar_ItensValidos_BaixaOEstoqueEGravaOsItensDoPedido()
    {
        // Arrange
        JogosNoBanco(CriarJogo(1, 100.00m, 5), CriarJogo(2, 59.90m, 5));
        List<ItemPedido>? itensGravados = null;
        itemPedidoRepository.Setup(r => r.InserirAsync(10, It.IsAny<List<ItemPedido>>()))
            .Callback<int, List<ItemPedido>>((_, itens) => itensGravados = itens)
            .Returns(Task.CompletedTask);
        var service = CriarService();

        // Act
        await service.CriarAsync(new List<ItemPedido> { Item(1, 2), Item(2, 1) });

        // Assert
        jogoRepository.Verify(r => r.BaixarEstoqueAsync(1, 2), Times.Once);
        jogoRepository.Verify(r => r.BaixarEstoqueAsync(2, 1), Times.Once);
        Assert.NotNull(itensGravados);
        Assert.Equal(2, itensGravados.Count);
        Assert.All(itensGravados, item => Assert.Equal(10, item.IdPedido));
    }

    [Fact]
    public async Task Criar_ItemComPrecoVindoDeFora_UsaOPrecoDoBanco()
    {
        // Arrange: o item chega com um preço qualquer; o jogo custa 149,90 no banco.
        JogosNoBanco(CriarJogo(1, 149.90m, 5));
        var item = Item(1, 2);
        item.ValorUnitario = 0.01m;
        Pedido? gravado = null;
        pedidoRepository.Setup(r => r.InserirAsync(It.IsAny<Pedido>()))
            .Callback<Pedido>(pedido => gravado = pedido)
            .ReturnsAsync(10);
        var service = CriarService();

        // Act
        var criado = await service.CriarAsync(new List<ItemPedido> { item });

        // Assert
        Assert.Equal(149.90m, criado.Itens[0].Item.ValorUnitario);
        Assert.Equal(299.80m, gravado!.Total);
    }

    [Fact]
    public async Task Criar_ItensValidos_DevolvePedidoComSituacaoEItensComNomeDoJogo()
    {
        // Arrange
        JogosNoBanco(CriarJogo(1, 100.00m, 5));
        var service = CriarService();

        // Act
        var criado = await service.CriarAsync(new List<ItemPedido> { Item(1, 3) });

        // Assert
        Assert.Equal(10, criado.Pedido.Id);
        Assert.Equal("Pendente", criado.DescricaoSituacao);
        var item = Assert.Single(criado.Itens);
        Assert.Equal("Jogo 1", item.NomeJogo);
        Assert.Equal(3, item.Item.Quantidade);
    }

    [Fact]
    public async Task Criar_JogoInexistente_LancaJogoIndisponivelENaoGrava()
    {
        // Arrange: o pedido tem os jogos 1 e 99; só o 1 existe.
        JogosNoBanco(CriarJogo(1, 100.00m, 5));
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(new List<ItemPedido> { Item(1, 1), Item(99, 1) }));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_INDISPONIVEL, excecao.Erro);
        NadaFoiGravado();
    }

    [Fact]
    public async Task Criar_JogoInativo_LancaJogoIndisponivelENaoGrava()
    {
        // Arrange
        JogosNoBanco(CriarJogo(1, 100.00m, 5, ativo: false));
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(new List<ItemPedido> { Item(1, 1) }));

        // Assert
        Assert.Equal(BusinessErrors.JOGO_INDISPONIVEL, excecao.Erro);
        NadaFoiGravado();
    }

    [Fact]
    public async Task Criar_QuantidadeUmAcimaDoEstoque_LancaEstoqueInsuficienteENaoGrava()
    {
        // Arrange: borda de cima, estoque 2 e quantidade 3.
        JogosNoBanco(CriarJogo(1, 100.00m, 2));
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(new List<ItemPedido> { Item(1, 3) }));

        // Assert
        Assert.Equal(BusinessErrors.ESTOQUE_INSUFICIENTE, excecao.Erro);
        NadaFoiGravado();
    }

    [Fact]
    public async Task Criar_QuantidadeIgualAoEstoque_GravaOPedido()
    {
        // Arrange: borda, estoque 3 e quantidade 3 (leva tudo).
        JogosNoBanco(CriarJogo(1, 100.00m, 3));
        var service = CriarService();

        // Act
        await service.CriarAsync(new List<ItemPedido> { Item(1, 3) });

        // Assert
        jogoRepository.Verify(r => r.BaixarEstoqueAsync(1, 3), Times.Once);
        pedidoRepository.Verify(r => r.InserirAsync(It.IsAny<Pedido>()), Times.Once);
    }

    [Fact]
    public async Task Criar_BaixaRecusadaPorCompraSimultanea_LancaEstoqueInsuficienteENaoInserePedido()
    {
        // Arrange: a conferência passou, mas outra compra levou o jogo 2 antes da baixa.
        JogosNoBanco(CriarJogo(1, 100.00m, 5), CriarJogo(2, 59.90m, 1));
        jogoRepository.Setup(r => r.BaixarEstoqueAsync(2, 1)).ReturnsAsync(false);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CriarAsync(new List<ItemPedido> { Item(1, 1), Item(2, 1) }));

        // Assert: a baixa do jogo 1 é desfeita pela transação; pedido e itens nem chegam a ser gravados.
        Assert.Equal(BusinessErrors.ESTOQUE_INSUFICIENTE, excecao.Erro);
        pedidoRepository.Verify(r => r.InserirAsync(It.IsAny<Pedido>()), Times.Never);
        itemPedidoRepository.Verify(r => r.InserirAsync(It.IsAny<int>(), It.IsAny<List<ItemPedido>>()), Times.Never);
    }

    [Fact]
    public async Task ListarMeus_UsuarioLogado_BuscaSoOsPedidosDele()
    {
        // Arrange
        pedidoRepository.Setup(r => r.ListarPorUsuarioAsync(It.IsAny<int>())).ReturnsAsync(new List<Pedido>());
        var service = CriarService();

        // Act
        await service.ListarMeusAsync();

        // Assert
        pedidoRepository.Verify(r => r.ListarPorUsuarioAsync(IdUsuarioLogado), Times.Once);
        pedidoRepository.Verify(r => r.ListarPorUsuarioAsync(It.Is<int>(id => id != IdUsuarioLogado)), Times.Never);
    }

    [Fact]
    public async Task ListarMeus_PedidosDoMaisRecenteAoMaisAntigo_MantemAOrdem()
    {
        // Arrange: o Repository devolve do mais recente (12) para o mais antigo (11).
        pedidoRepository.Setup(r => r.ListarPorUsuarioAsync(IdUsuarioLogado)).ReturnsAsync(new List<Pedido>
        {
            new() { Id = 12, Data = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc), IdSituacaoPedido = SituacaoPedidoIds.Pago },
            new() { Id = 11, Data = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc), IdSituacaoPedido = SituacaoPedidoIds.Pendente },
        });
        itemPedidoRepository.Setup(r => r.ListarPorPedidosAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<ItemPedido>());
        JogosNoBanco();
        var service = CriarService();

        // Act
        var pedidos = await service.ListarMeusAsync();

        // Assert
        Assert.Equal(new[] { 12, 11 }, pedidos.Select(pedido => pedido.Pedido.Id));
        Assert.Equal(new[] { "Pago", "Pendente" }, pedidos.Select(pedido => pedido.DescricaoSituacao));
    }

    [Fact]
    public async Task ListarMeus_ItensDeCadaPedido_ComNomeDoJogoEPrecoPago()
    {
        // Arrange: o jogo 1 custava 100,00 na compra e hoje custa 150,00.
        pedidoRepository.Setup(r => r.ListarPorUsuarioAsync(IdUsuarioLogado)).ReturnsAsync(new List<Pedido>
        {
            new() { Id = 12, IdSituacaoPedido = SituacaoPedidoIds.Pendente },
            new() { Id = 11, IdSituacaoPedido = SituacaoPedidoIds.Pendente },
        });
        itemPedidoRepository.Setup(r => r.ListarPorPedidosAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<ItemPedido>
        {
            new() { Id = 1, IdPedido = 11, IdJogo = 1, Quantidade = 2, ValorUnitario = 100.00m },
            new() { Id = 2, IdPedido = 12, IdJogo = 2, Quantidade = 1, ValorUnitario = 59.90m },
        });
        JogosNoBanco(CriarJogo(1, 150.00m, 5), CriarJogo(2, 59.90m, 5));
        var service = CriarService();

        // Act
        var pedidos = await service.ListarMeusAsync();

        // Assert
        var itemDoPedido11 = Assert.Single(pedidos.Single(pedido => pedido.Pedido.Id == 11).Itens);
        Assert.Equal("Jogo 1", itemDoPedido11.NomeJogo);
        Assert.Equal(2, itemDoPedido11.Item.Quantidade);
        Assert.Equal(100.00m, itemDoPedido11.Item.ValorUnitario);
        var itemDoPedido12 = Assert.Single(pedidos.Single(pedido => pedido.Pedido.Id == 12).Itens);
        Assert.Equal("Jogo 2", itemDoPedido12.NomeJogo);
    }

    [Fact]
    public async Task ListarMeus_SemPedidos_DevolveListaVaziaSemConsultarItens()
    {
        // Arrange
        pedidoRepository.Setup(r => r.ListarPorUsuarioAsync(IdUsuarioLogado)).ReturnsAsync(new List<Pedido>());
        var service = CriarService();

        // Act
        var pedidos = await service.ListarMeusAsync();

        // Assert
        Assert.Empty(pedidos);
        itemPedidoRepository.Verify(r => r.ListarPorPedidosAsync(It.IsAny<List<int>>()), Times.Never);
    }

    /// <summary>O pedido 20 é do usuário informado, com um item do jogo 1 (2 × 100,00).</summary>
    private void PedidoNoBanco(int idDono, int idSituacao = SituacaoPedidoIds.Pendente)
    {
        pedidoRepository.Setup(r => r.BuscarPorIdAsync(20)).ReturnsAsync(new Pedido
        {
            Id = 20, IdUsuario = idDono, Total = 200.00m, IdSituacaoPedido = idSituacao,
            Data = new DateTime(2026, 11, 25, 14, 0, 0, DateTimeKind.Utc),
        });
        itemPedidoRepository.Setup(r => r.ListarPorPedidosAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<ItemPedido>
        {
            new() { Id = 1, IdPedido = 20, IdJogo = 1, Quantidade = 2, ValorUnitario = 100.00m },
        });
        JogosNoBanco(CriarJogo(1, 100.00m, 5));
    }

    [Fact]
    public async Task Obter_PedidoDoProprioCliente_DevolveSituacaoDataItensETotal()
    {
        // Arrange
        PedidoNoBanco(idDono: IdUsuarioLogado);
        var service = CriarService();

        // Act
        var detalhe = await service.ObterAsync(20);

        // Assert
        Assert.Equal(20, detalhe.Pedido.Id);
        Assert.Equal("Pendente", detalhe.DescricaoSituacao);
        Assert.Equal(new DateTime(2026, 11, 25, 14, 0, 0, DateTimeKind.Utc), detalhe.Pedido.Data);
        Assert.Equal(200.00m, detalhe.Pedido.Total);
        var item = Assert.Single(detalhe.Itens);
        Assert.Equal("Jogo 1", item.NomeJogo);
        Assert.Equal(2, item.Item.Quantidade);
    }

    [Fact]
    public async Task Obter_PedidoDeOutroCliente_LancaPedidoDeOutroUsuario()
    {
        // Arrange: o pedido é do usuário 99; o logado (5) é cliente.
        PedidoNoBanco(idDono: 99);
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(false);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.ObterAsync(20));

        // Assert: e nem chega a buscar os itens do pedido alheio.
        Assert.Equal(BusinessErrors.PEDIDO_DE_OUTRO_USUARIO, excecao.Erro);
        itemPedidoRepository.Verify(r => r.ListarPorPedidosAsync(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task Obter_AdministradorEPedidoDeOutroUsuario_DevolveODetalhe()
    {
        // Arrange
        PedidoNoBanco(idDono: 99);
        usuarioAtual.Setup(u => u.EhAdministrador).Returns(true);
        var service = CriarService();

        // Act
        var detalhe = await service.ObterAsync(20);

        // Assert
        Assert.Equal(20, detalhe.Pedido.Id);
        Assert.Equal(99, detalhe.Pedido.IdUsuario);
    }

    [Fact]
    public async Task Obter_PedidoInexistente_LancaPedidoNaoEncontrado()
    {
        // Arrange
        pedidoRepository.Setup(r => r.BuscarPorIdAsync(It.IsAny<int>())).ReturnsAsync((Pedido?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.ObterAsync(999));

        // Assert
        Assert.Equal(BusinessErrors.PEDIDO_NAO_ENCONTRADO, excecao.Erro);
    }

    /// <summary>Dois pedidos de clientes diferentes, com os itens e os clientes no banco.</summary>
    private void PedidosDeDoisClientesNoBanco()
    {
        pedidoRepository.Setup(r => r.ListarAsync(It.IsAny<FiltroPedido>())).ReturnsAsync(new List<Pedido>
        {
            new() { Id = 31, IdUsuario = 7, IdSituacaoPedido = SituacaoPedidoIds.Pago },
            new() { Id = 30, IdUsuario = 8, IdSituacaoPedido = SituacaoPedidoIds.Pendente },
        });
        itemPedidoRepository.Setup(r => r.ListarPorPedidosAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<ItemPedido>
        {
            new() { Id = 1, IdPedido = 30, IdJogo = 1, Quantidade = 1, ValorUnitario = 100.00m },
            new() { Id = 2, IdPedido = 31, IdJogo = 1, Quantidade = 2, ValorUnitario = 100.00m },
        });
        JogosNoBanco(CriarJogo(1, 100.00m, 5));
        usuarioRepository.Setup(r => r.ListarPorIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Usuario>
        {
            new() { Id = 7, Nome = "Ana" },
            new() { Id = 8, Nome = "Bruno" },
        });
    }

    [Fact]
    public async Task Listar_SemFiltro_DevolveTodosOsPedidosComONomeDoCliente()
    {
        // Arrange
        PedidosDeDoisClientesNoBanco();
        var service = CriarService();

        // Act
        var pedidos = await service.ListarAsync(new FiltroPedido());

        // Assert: o Repository recebe o filtro sem situação e a ordem dele é mantida.
        pedidoRepository.Verify(r => r.ListarAsync(It.Is<FiltroPedido>(f => f.IdSituacaoPedido == null)), Times.Once);
        Assert.Equal(new[] { 31, 30 }, pedidos.Select(pedido => pedido.Pedido.Id));
        Assert.Equal(new[] { "Ana", "Bruno" }, pedidos.Select(pedido => pedido.NomeUsuario));
        Assert.Equal(2, pedidos.Single(pedido => pedido.Pedido.Id == 31).Itens.Single().Item.Quantidade);
    }

    [Fact]
    public async Task Listar_FiltroPorSituacao_RepassaASituacaoAoRepository()
    {
        // Arrange
        PedidosDeDoisClientesNoBanco();
        var service = CriarService();

        // Act
        await service.ListarAsync(new FiltroPedido { IdSituacaoPedido = SituacaoPedidoIds.Pago });

        // Assert
        pedidoRepository.Verify(r => r.ListarAsync(
            It.Is<FiltroPedido>(f => f.IdSituacaoPedido == SituacaoPedidoIds.Pago)), Times.Once);
    }

    [Fact]
    public async Task Listar_FiltroComSituacaoInexistente_LancaSituacaoNaoEncontradaSemListar()
    {
        // Arrange
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.ListarAsync(new FiltroPedido { IdSituacaoPedido = 99 }));

        // Assert
        Assert.Equal(BusinessErrors.SITUACAO_NAO_ENCONTRADA, excecao.Erro);
        pedidoRepository.Verify(r => r.ListarAsync(It.IsAny<FiltroPedido>()), Times.Never);
    }

    [Fact]
    public async Task AlterarSituacao_PedidoESituacaoExistentes_GravaEDevolveOPedidoAtualizado()
    {
        // Arrange: o pedido 20 está Pendente e passa a Pago.
        PedidoNoBanco(idDono: 99);
        pedidoRepository.Setup(r => r.AlterarSituacaoAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        var service = CriarService();

        // Act
        var atualizado = await service.AlterarSituacaoAsync(20, SituacaoPedidoIds.Pago);

        // Assert
        pedidoRepository.Verify(r => r.AlterarSituacaoAsync(20, SituacaoPedidoIds.Pago), Times.Once);
        Assert.Equal(SituacaoPedidoIds.Pago, atualizado.Pedido.IdSituacaoPedido);
        Assert.Equal("Pago", atualizado.DescricaoSituacao);
        Assert.Single(atualizado.Itens);
    }

    [Fact]
    public async Task AlterarSituacao_PedidoInexistente_LancaPedidoNaoEncontradoSemGravar()
    {
        // Arrange: situação também inexistente, para provar que o pedido é conferido primeiro.
        pedidoRepository.Setup(r => r.BuscarPorIdAsync(It.IsAny<int>())).ReturnsAsync((Pedido?)null);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.AlterarSituacaoAsync(999, 99));

        // Assert
        Assert.Equal(BusinessErrors.PEDIDO_NAO_ENCONTRADO, excecao.Erro);
        pedidoRepository.Verify(r => r.AlterarSituacaoAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task AlterarSituacao_SituacaoInexistente_LancaSituacaoNaoEncontradaSemGravar()
    {
        // Arrange
        PedidoNoBanco(idDono: 99);
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(() => service.AlterarSituacaoAsync(20, 99));

        // Assert
        Assert.Equal(BusinessErrors.SITUACAO_NAO_ENCONTRADA, excecao.Erro);
        pedidoRepository.Verify(r => r.AlterarSituacaoAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
