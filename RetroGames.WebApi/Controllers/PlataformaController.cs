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

    /// <summary>POST /api/Plataforma: 201, a plataforma criada em data e o Location /api/Plataforma/{id}.</summary>
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>>> Inserir(
        InserirPlataformaRequestApiModel request)
    {
        var criada = await plataformaService.InserirAsync(request.ParaModel());
        return CreatedAtAction(nameof(ListarPorId), new { id = criada.Id },
            new RetroGamesResponseApiModel<ListarPlataformaResponseApiModel>(criada.ParaResponse()));
    }

    /// <summary>PUT /api/Plataforma/{id}: altera a plataforma do id da rota.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Alterar(int id, AlterarPlataformaRequestApiModel request)
    {
        await plataformaService.AlterarAsync(request.ParaModel(id));
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }

    /// <summary>DELETE /api/Plataforma/{id}: apaga a plataforma sem jogos. Responde 200 com o envelope (nunca 204).</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Excluir(int id)
    {
        await plataformaService.ExcluirAsync(id);
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }
}
