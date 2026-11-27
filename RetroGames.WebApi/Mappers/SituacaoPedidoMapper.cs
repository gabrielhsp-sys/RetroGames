using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversão, escrita à mão, entre SituacaoPedido e os ApiModels de situação.</summary>
public static class SituacaoPedidoMapper
{
    /// <summary>SituacaoPedido do modelo → ApiModel que sai no JSON.</summary>
    public static ListarSituacaoPedidoResponseApiModel ParaResponse(this SituacaoPedido situacaoPedido) => new()
    {
        Id = situacaoPedido.Id,
        Descricao = situacaoPedido.Descricao,
    };
}
