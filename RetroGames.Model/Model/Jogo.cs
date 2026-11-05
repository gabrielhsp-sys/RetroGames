namespace RetroGames.Model.Model;

/// <summary>Tabela tb_jogo.</summary>
public class Jogo
{
    /// <summary>Coluna co_seq_jogo.</summary>
    public int Id { get; set; }

    /// <summary>Coluna no_jogo.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Coluna ds_jogo (opcional).</summary>
    public string? Descricao { get; set; }

    /// <summary>Coluna nu_ano_lancamento.</summary>
    public int AnoLancamento { get; set; }

    /// <summary>Coluna vl_preco.</summary>
    public decimal Preco { get; set; }

    /// <summary>Coluna qt_estoque.</summary>
    public int Estoque { get; set; }

    /// <summary>Coluna st_ativo.</summary>
    public bool Ativo { get; set; }

    /// <summary>Coluna dt_cadastro (data e hora em UTC).</summary>
    public DateTime Cadastro { get; set; }

    /// <summary>Coluna co_plataforma (chave estrangeira para tb_plataforma).</summary>
    public int IdPlataforma { get; set; }

    /// <summary>Coluna co_condicao_jogo (chave estrangeira para tb_condicao_jogo).</summary>
    public int IdCondicaoJogo { get; set; }
}
