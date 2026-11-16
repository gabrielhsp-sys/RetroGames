using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Catálogo de jogos: listagem paginada com filtros e detalhe. Recebe, chama o
/// Service, converte e responde: sem SQL, sem regra e sem try/catch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JogoController(IJogoService jogoService) : ControllerBase
{
    /// <summary>GET /api/Jogo?busca=&amp;idPlataforma=&amp;idGenero=&amp;pagina=&amp;tamanhoPagina=</summary>
    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<PaginaResponseApiModel<ListarJogoResponseApiModel>>>> Listar(
        [FromQuery] ListarJogoRequestApiModel request)
    {
        var (itens, totalItens) = await jogoService.ListarAsync(
            request.Busca, request.IdPlataforma, request.IdGenero, request.Pagina, request.TamanhoPagina);
        return Ok(new RetroGamesResponseApiModel<PaginaResponseApiModel<ListarJogoResponseApiModel>>(
            itens.ParaPaginaResponse(totalItens, request)));
    }

    /// <summary>GET /api/Jogo/{id}: o detalhe de um jogo.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> ListarPorId(int id)
    {
        var detalhe = await jogoService.BuscarPorIdAsync(id);
        return Ok(new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>(detalhe.ParaListarPorIdResponse()));
    }
}
