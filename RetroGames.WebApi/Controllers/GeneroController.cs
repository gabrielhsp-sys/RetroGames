using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>Consulta de gêneros (somente leitura). Mesmo desenho do PlataformaController.</summary>
[ApiController]
[Route("api/[controller]")]
public class GeneroController(IGeneroService generoService) : ControllerBase
{
    /// <summary>GET /api/Genero: todos os gêneros.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarGeneroResponseApiModel>>>> Listar()
    {
        var generos = await generoService.ListarAsync();
        return Ok(new RetroGamesResponseApiModel<List<ListarGeneroResponseApiModel>>(
            generos.Select(genero => genero.ParaResponse()).ToList()));
    }
}
