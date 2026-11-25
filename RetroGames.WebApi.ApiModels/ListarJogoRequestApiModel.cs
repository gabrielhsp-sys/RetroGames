namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Filtros e paginação do catálogo, lidos da query string de GET /api/Jogo
/// (ex.: ?busca=zelda&amp;idPlataforma=2&amp;pagina=1). Cada filtro só vale quando informado.
/// </summary>
public class ListarJogoRequestApiModel
{
    /// <summary>Trecho do nome do jogo.</summary>
    public string? Busca { get; set; }

    /// <summary>Só jogos desta plataforma.</summary>
    public int? IdPlataforma { get; set; }

    /// <summary>Só jogos deste gênero.</summary>
    public int? IdGenero { get; set; }

    /// <summary>Número da página, a partir de 1.</summary>
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de jogos por página.</summary>
    public int TamanhoPagina { get; set; } = 12;

    /// <summary>Incluir também os jogos inativos. Só vale para o administrador (RN04).</summary>
    public bool IncluirInativos { get; set; }
}
