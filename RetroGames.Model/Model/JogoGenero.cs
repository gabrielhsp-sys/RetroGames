namespace RetroGames.Model.Model;

/// <summary>Tabela tb_jogo_genero (chave primária composta por co_jogo e co_genero).</summary>
public class JogoGenero
{
    /// <summary>Coluna co_jogo (chave estrangeira para tb_jogo).</summary>
    public int IdJogo { get; set; }

    /// <summary>Coluna co_genero (chave estrangeira para tb_genero).</summary>
    public int IdGenero { get; set; }
}
