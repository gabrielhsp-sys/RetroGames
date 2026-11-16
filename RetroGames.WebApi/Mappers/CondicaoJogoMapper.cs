using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversão, escrita à mão, entre CondicaoJogo e os ApiModels de condição.</summary>
public static class CondicaoJogoMapper
{
    /// <summary>CondicaoJogo do modelo → ApiModel que sai no JSON.</summary>
    public static ListarCondicaoJogoResponseApiModel ParaResponse(this CondicaoJogo condicaoJogo) => new()
    {
        Id = condicaoJogo.Id,
        Descricao = condicaoJogo.Descricao,
    };
}
