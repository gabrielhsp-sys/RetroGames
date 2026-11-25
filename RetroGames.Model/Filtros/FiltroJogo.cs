namespace RetroGames.Model.Filtros;

/// <summary>
/// Filtros e paginação da listagem de jogos. A API monta com o que veio na query string;
/// o JogoService resolve IdGenero em Ids (LINQ) e decide IncluirInativos (RN04);
/// o JogoRepository usa o resto no SQL. Não é tabela: por isso fica fora da pasta Model.
/// </summary>
public class FiltroJogo
{
    /// <summary>Trecho do nome do jogo.</summary>
    public string? Busca { get; set; }

    /// <summary>Só jogos desta plataforma.</summary>
    public int? IdPlataforma { get; set; }

    /// <summary>Só jogos deste gênero. O Repository não usa: o Service o converte em Ids.</summary>
    public int? IdGenero { get; set; }

    /// <summary>Só jogos com estes ids. Nulo: sem restrição; lista vazia: nenhum jogo.</summary>
    public List<int>? Ids { get; set; }

    /// <summary>Se os jogos inativos também entram. Só vale para o administrador (RN04).</summary>
    public bool IncluirInativos { get; set; }

    /// <summary>Número da página, a partir de 1.</summary>
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de jogos por página.</summary>
    public int TamanhoPagina { get; set; } = 12;
}
