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
}
