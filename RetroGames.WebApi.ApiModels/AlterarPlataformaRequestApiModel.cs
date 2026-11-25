namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de PUT /api/Plataforma/{id}. O id vem da rota, nunca do corpo.</summary>
public class AlterarPlataformaRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public int AnoLancamento { get; set; }
}
