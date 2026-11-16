namespace RetroGames.WebApi.ApiModels;

/// <summary>Um gênero, como sai no JSON de GET /api/Genero.</summary>
public class ListarGeneroResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
