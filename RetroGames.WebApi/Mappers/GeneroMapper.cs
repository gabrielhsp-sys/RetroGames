using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversão, escrita à mão, entre Genero e os ApiModels de gênero.</summary>
public static class GeneroMapper
{
    /// <summary>Genero do modelo → ApiModel que sai no JSON.</summary>
    public static ListarGeneroResponseApiModel ParaResponse(this Genero genero) => new()
    {
        Id = genero.Id,
        Nome = genero.Nome,
    };
}
