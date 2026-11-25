using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Consulta de condições de jogo (somente leitura). A rota /api/CondicaoJogo vem
/// do nome da classe, por [controller]: não há texto fixo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CondicaoJogoController(ICondicaoJogoService condicaoJogoService) : ControllerBase
{
    /// <summary>GET /api/CondicaoJogo: todas as condições de jogo.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarCondicaoJogoResponseApiModel>>>> Listar()
    {
        var condicoes = await condicaoJogoService.ListarAsync();
        return Ok(new RetroGamesResponseApiModel<List<ListarCondicaoJogoResponseApiModel>>(
            condicoes.Select(condicaoJogo => condicaoJogo.ParaResponse()).ToList()));
    }
}
