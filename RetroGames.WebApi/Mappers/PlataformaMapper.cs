using RetroGames.Model.Model;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>Conversão, escrita à mão, entre Plataforma e os ApiModels de plataforma.</summary>
public static class PlataformaMapper
{
    /// <summary>Plataforma do modelo → ApiModel que sai no JSON.</summary>
    public static ListarPlataformaResponseApiModel ParaResponse(this Plataforma plataforma) => new()
    {
        Id = plataforma.Id,
        Nome = plataforma.Nome,
        Fabricante = plataforma.Fabricante,
        AnoLancamento = plataforma.AnoLancamento,
    };

    /// <summary>Corpo do POST → Plataforma do modelo. O id ainda não existe: o banco o gera.</summary>
    public static Plataforma ParaModel(this InserirPlataformaRequestApiModel request) => new()
    {
        Nome = request.Nome,
        Fabricante = request.Fabricante,
        AnoLancamento = request.AnoLancamento,
    };

    /// <summary>Corpo do PUT → Plataforma do modelo. O id vem da rota, não do corpo.</summary>
    public static Plataforma ParaModel(this AlterarPlataformaRequestApiModel request, int id) => new()
    {
        Id = id,
        Nome = request.Nome,
        Fabricante = request.Fabricante,
        AnoLancamento = request.AnoLancamento,
    };
}
