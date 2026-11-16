namespace RetroGames.WebApi.ApiModels;

/// <summary>Uma página de resultados e os totais para montar a paginação.</summary>
public class PaginaResponseApiModel<T>
{
    /// <summary>Os registros desta página.</summary>
    public List<T> Itens { get; set; } = new();

    /// <summary>Número desta página, a partir de 1.</summary>
    public int Pagina { get; set; }

    /// <summary>Quantidade pedida por página.</summary>
    public int TamanhoPagina { get; set; }

    /// <summary>Total de registros encontrados, somando todas as páginas.</summary>
    public int TotalItens { get; set; }

    /// <summary>Quantidade de páginas para o total encontrado.</summary>
    public int TotalPaginas { get; set; }
}
