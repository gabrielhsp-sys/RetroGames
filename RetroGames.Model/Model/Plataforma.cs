namespace RetroGames.Model.Model;

/// <summary>Tabela tb_plataforma.</summary>
public class Plataforma
{
    /// <summary>Coluna co_seq_plataforma.</summary>
    public int Id { get; set; }

    /// <summary>Coluna no_plataforma.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Coluna no_fabricante.</summary>
    public string Fabricante { get; set; } = string.Empty;

    /// <summary>Coluna nu_ano_lancamento.</summary>
    public int AnoLancamento { get; set; }
}
