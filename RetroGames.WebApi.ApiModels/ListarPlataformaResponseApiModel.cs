namespace RetroGames.WebApi.ApiModels;

/// <summary>Uma plataforma, como sai no JSON das consultas de plataformas.</summary>
public class ListarPlataformaResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public int AnoLancamento { get; set; }
}
