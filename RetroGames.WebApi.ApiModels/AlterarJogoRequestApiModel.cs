namespace RetroGames.WebApi.ApiModels;

/// <summary>Corpo de PUT /api/Jogo/{id}. O id vem da rota, nunca do corpo.</summary>
public class AlterarJogoRequestApiModel
{
    public string Nome { get; set; } = string.Empty;

    /// <summary>Opcional: vazia é gravada como NULL.</summary>
    public string? Descricao { get; set; }

    public int AnoLancamento { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public int IdPlataforma { get; set; }
    public int IdCondicaoJogo { get; set; }

    /// <summary>Ids dos gêneros do jogo; ao menos um (RN05). Vão ao Service separados do jogo.</summary>
    public List<int> IdsGeneros { get; set; } = new();

    /// <summary>Situação do jogo: false desativa, true reativa (RN03).</summary>
    public bool Ativo { get; set; }
}
