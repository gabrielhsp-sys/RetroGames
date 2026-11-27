namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de PUT /api/Genero/{id}. O id vem da rota, nunca do corpo.</summary>
public class AlterarGeneroRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
}
