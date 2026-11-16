using RetroGames.Services;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Mappers;

/// <summary>
/// Conversão, escrita à mão, do JogoDetalheServiceModel (jogo já combinado com
/// plataforma, condição e gêneros pelo JogoService) nos ApiModels de saída.
/// </summary>
public static class JogoMapper
{
    /// <summary>Um jogo da listagem do catálogo.</summary>
    public static ListarJogoResponseApiModel ParaResponse(this JogoDetalheServiceModel detalhe) => new()
    {
        Id = detalhe.Jogo.Id,
        Nome = detalhe.Jogo.Nome,
        AnoLancamento = detalhe.Jogo.AnoLancamento,
        Preco = detalhe.Jogo.Preco,
        Estoque = detalhe.Jogo.Estoque,
        Ativo = detalhe.Jogo.Ativo,
        NomePlataforma = detalhe.NomePlataforma,
        DescricaoCondicao = detalhe.DescricaoCondicao,
        Generos = detalhe.Generos.Select(genero => genero.Nome).ToList(),
    };

    /// <summary>O detalhe de um jogo, com ids de plataforma e condição e os gêneros com id e nome.</summary>
    public static ListarPorIdJogoResponseApiModel ParaListarPorIdResponse(this JogoDetalheServiceModel detalhe) => new()
    {
        Id = detalhe.Jogo.Id,
        Nome = detalhe.Jogo.Nome,
        Descricao = detalhe.Jogo.Descricao,
        AnoLancamento = detalhe.Jogo.AnoLancamento,
        Preco = detalhe.Jogo.Preco,
        Estoque = detalhe.Jogo.Estoque,
        Ativo = detalhe.Jogo.Ativo,
        IdPlataforma = detalhe.Jogo.IdPlataforma,
        NomePlataforma = detalhe.NomePlataforma,
        IdCondicaoJogo = detalhe.Jogo.IdCondicaoJogo,
        DescricaoCondicao = detalhe.DescricaoCondicao,
        Generos = detalhe.Generos.Select(genero => genero.ParaResponse()).ToList(),
    };

    /// <summary>Uma página do catálogo: os jogos desta página e os totais.</summary>
    public static PaginaResponseApiModel<ListarJogoResponseApiModel> ParaPaginaResponse(
        this List<JogoDetalheServiceModel> itens, int totalItens, ListarJogoRequestApiModel request) => new()
    {
        Itens = itens.Select(detalhe => detalhe.ParaResponse()).ToList(),
        Pagina = request.Pagina,
        TamanhoPagina = request.TamanhoPagina,
        TotalItens = totalItens,
        // Arredonda para cima: 20 jogos em páginas de 12 dão 2 páginas.
        TotalPaginas = request.TamanhoPagina > 0
            ? (int)Math.Ceiling(totalItens / (double)request.TamanhoPagina)
            : 0,
    };
}
