namespace RetroGames.Model.Model;

/// <summary>Tabela tb_condicao_jogo.</summary>
public class CondicaoJogo
{
    /// <summary>Coluna co_condicao_jogo (chave fixa).</summary>
    public int Id { get; set; }

    /// <summary>Coluna ds_condicao_jogo.</summary>
    public string Descricao { get; set; } = string.Empty;
}
