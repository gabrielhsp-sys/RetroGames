namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de POST /api/Plataforma: só o que o cliente pode enviar (sem id).</summary>
public class InserirPlataformaRequestApiModel
{
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public int AnoLancamento { get; set; }
}
