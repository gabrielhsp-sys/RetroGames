using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Consultas de plataformas. Recebe, chama o Service, converte e responde:
/// sem SQL, sem regra e sem try/catch (erros vão para o GlobalExceptionHandler).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlataformaController(IPlataformaService plataformaService) : ControllerBase
{
    /// <summary>GET /api/Plataforma: todas as plataformas.</summary>
    [HttpGet]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarPlataformaResponseApiModel>>>> Listar()
    {
        var plataformas = await plataformaService.ListarAsync();
        return Ok(new RetroGamesResponseApiModel<List<ListarPlataformaResponseApiModel>>(
            plataformas.Select(plataforma => plataforma.ParaResponse()).ToList()));
    }

    /// <summary>GET /api/Plataforma/{id}: uma plataforma.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> ListarPorId(int id)
    {
        var plataforma = await plataformaService.BuscarPorIdAsync(id);
        return Ok(new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>(plataforma.ParaResponse()));
    }
}
