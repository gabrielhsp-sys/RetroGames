namespace RetroGames.WebApi.ApiModels;

/// <summary>Um jogo na listagem do catálogo (GET /api/Jogo).</summary>
public class ListarJogoResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int AnoLancamento { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public bool Ativo { get; set; }

    /// <summary>Nome da plataforma do jogo (vem de tb_plataforma).</summary>
    public string NomePlataforma { get; set; } = string.Empty;

    /// <summary>Descrição da condição do jogo (vem de tb_condicao_jogo).</summary>
    public string DescricaoCondicao { get; set; } = string.Empty;

    /// <summary>Nomes dos gêneros do jogo.</summary>
    public List<string> Generos { get; set; } = new();
}
