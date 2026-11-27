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

    /// <summary>Corpo do POST → Genero do modelo. O id ainda não existe: o banco o gera.</summary>
    public static Genero ParaModel(this InserirGeneroRequestApiModel request) => new()
    {
        Nome = request.Nome,
    };

    /// <summary>Corpo do PUT → Genero do modelo. O id vem da rota, não do corpo.</summary>
    public static Genero ParaModel(this AlterarGeneroRequestApiModel request, int id) => new()
    {
        Id = id,
        Nome = request.Nome,
    };
}
