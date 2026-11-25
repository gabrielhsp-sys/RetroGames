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

    /// <summary>POST /api/Jogo: grava o jogo e os gêneros (numa transação, no Service). 201 com o detalhe e o Location.</summary>
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>>> Inserir(
        InserirJogoRequestApiModel request)
    {
        var criado = await jogoService.InserirAsync(request.ParaModel(), request.IdsGeneros);
        var detalhe = await jogoService.BuscarPorIdAsync(criado.Id);
        return CreatedAtAction(nameof(ListarPorId), new { id = criado.Id },
            new RetroGamesResponseApiModel<ListarPorIdJogoResponseApiModel>(detalhe.ParaListarPorIdResponse()));
    }

    /// <summary>PUT /api/Jogo/{id}: altera o jogo do id da rota e troca a lista de gêneros.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Alterar(int id, AlterarJogoRequestApiModel request)
    {
        await jogoService.AlterarAsync(request.ParaModel(id), request.IdsGeneros);
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }

    /// <summary>DELETE /api/Jogo/{id}: só desativa o jogo, nunca o apaga (RN03). Responde 200 com o envelope.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel>> Excluir(int id)
    {
        await jogoService.DesativarAsync(id);
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }
}
