using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Consultas (públicas) e gravação (só administrador) de gêneros (C08). Mesmo desenho do PlataformaController:
/// sem SQL, sem regra e sem try/catch (erros vão para o GlobalExceptionHandler).
/// </summary>
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

    /// <summary>GET /api/Genero/{id}: um gênero.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarGeneroResponseApiModel>>> ListarPorId(int id)
    {
        var genero = await generoService.BuscarPorIdAsync(id);
        return Ok(new RetroGamesResponseApiModel<ListarGeneroResponseApiModel>(genero.ParaResponse()));
    }

    /// <summary>POST /api/Genero: 201, o gênero criado em data e o Location /api/Genero/{id}.</summary>
    [HttpPost]
    [Authorize(Policy = Politicas.Administrador)]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarGeneroResponseApiModel>>> Inserir(
        InserirGeneroRequestApiModel request)
    {
        var criado = await generoService.InserirAsync(request.ParaModel());
        return CreatedAtAction(nameof(ListarPorId), new { id = criado.Id },
            new RetroGamesResponseApiModel<ListarGeneroResponseApiModel>(criado.ParaResponse()));
    }

    /// <summary>PUT /api/Genero/{id}: altera o gênero do id da rota.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Administrador)]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Alterar(int id, AlterarGeneroRequestApiModel request)
    {
        await generoService.AlterarAsync(request.ParaModel(id));
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }

    /// <summary>DELETE /api/Genero/{id}: apaga o gênero sem jogos. Responde 200 com o envelope (nunca 204).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Administrador)]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Excluir(int id)
    {
        await generoService.ExcluirAsync(id);
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }
}
