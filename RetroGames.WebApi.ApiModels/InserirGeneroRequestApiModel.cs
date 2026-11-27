namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de POST /api/Genero: só o que o cliente pode enviar (sem id).</summary>
public class InserirGeneroRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
}
