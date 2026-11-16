namespace RetroGames.WebApi.ApiModels;

/// <summary>O detalhe de um jogo (GET /api/Jogo/{id}).</summary>
public class ListarPorIdJogoResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int AnoLancamento { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public bool Ativo { get; set; }
    public int IdPlataforma { get; set; }

    /// <summary>Nome da plataforma do jogo (vem de tb_plataforma).</summary>
    public string NomePlataforma { get; set; } = string.Empty;

    public int IdCondicaoJogo { get; set; }

    /// <summary>Descrição da condição do jogo (vem de tb_condicao_jogo).</summary>
    public string DescricaoCondicao { get; set; } = string.Empty;

    /// <summary>Gêneros do jogo, com id e nome.</summary>
    public List<ListarGeneroResponseApiModel> Generos { get; set; } = new();
}
