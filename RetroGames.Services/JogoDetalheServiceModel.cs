using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>
/// Um jogo com os dados que vêm de outras tabelas, combinados com LINQ no JogoService:
/// o nome da plataforma, a descrição da condição e a lista de gêneros.
/// </summary>
public class JogoDetalheServiceModel
{
    /// <summary>O jogo, como o Repository entrega (tb_jogo).</summary>
    public Jogo Jogo { get; set; } = new();

    /// <summary>Nome da plataforma do jogo (tb_plataforma).</summary>
    public string NomePlataforma { get; set; } = string.Empty;

    /// <summary>Descrição da condição do jogo (tb_condicao_jogo).</summary>
    public string DescricaoCondicao { get; set; } = string.Empty;

    /// <summary>Gêneros do jogo (tb_genero, pelos vínculos de tb_jogo_genero).</summary>
    public List<Genero> Generos { get; set; } = new();
}
