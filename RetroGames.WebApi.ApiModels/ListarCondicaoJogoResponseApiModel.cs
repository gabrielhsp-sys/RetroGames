namespace RetroGames.WebApi.ApiModels;

/// <summary>Uma condição de jogo, como sai no JSON de GET /api/CondicaoJogo.</summary>
public class ListarCondicaoJogoResponseApiModel
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
