using RetroGames.Model.Filtros;
using RetroGames.Model.Model;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversão, escrita à mão, entre os ApiModels de pedido e o modelo / PedidoDetalheServiceModel.</summary>
public static class PedidoMapper
{
    /// <summary>Corpo do POST → itens do modelo, só com jogo e quantidade. O preço quem põe é o Service.</summary>
    public static List<ItemPedido> ParaModel(this InserirPedidoRequestApiModel request) =>
        request.Itens.Select(item => new ItemPedido
        {
            IdJogo = item.IdJogo,
            Quantidade = item.Quantidade,
        }).ToList();

    /// <summary>Query string de GET /api/Pedido → filtro do Service e do Repository.</summary>
    public static FiltroPedido ParaFiltro(this ListarPedidoRequestApiModel request) => new()
    {
        IdSituacaoPedido = request.IdSituacao,
    };

    /// <summary>Um pedido da gestão (C07) → ApiModel de saída com o nome do cliente.</summary>
    public static ListarTodosPedidoResponseApiModel ParaListarTodosResponse(this PedidoDetalheServiceModel detalhe)
    {
        var pedido = detalhe.ParaResponse();
        return new ListarTodosPedidoResponseApiModel
        {
            Id = pedido.Id,
            Data = pedido.Data,
            Total = pedido.Total,
            IdSituacaoPedido = pedido.IdSituacaoPedido,
            DescricaoSituacao = pedido.DescricaoSituacao,
            Itens = pedido.Itens,
            NomeUsuario = detalhe.NomeUsuario,
        };
    }

    /// <summary>Um pedido já combinado pelo PedidoService → ApiModel de saída.</summary>
    public static ListarPedidoResponseApiModel ParaResponse(this PedidoDetalheServiceModel detalhe) => new()
    {
        Id = detalhe.Pedido.Id,
        Data = detalhe.Pedido.Data,
        Total = detalhe.Pedido.Total,
        IdSituacaoPedido = detalhe.Pedido.IdSituacaoPedido,
        DescricaoSituacao = detalhe.DescricaoSituacao,
        Itens = detalhe.Itens.Select(item => new ListarItemPedidoResponseApiModel
        {
            IdJogo = item.Item.IdJogo,
            NomeJogo = item.NomeJogo,
            Quantidade = item.Item.Quantidade,
            ValorUnitario = item.Item.ValorUnitario,
        }).ToList(),
    };
}
