using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Pedidos do Módulo de Compras. Sem [AllowAnonymous]: toda action exige login pela
/// política padrão (401, código 4); a gestão (C07) exige o administrador (403, código 5). Recebe, chama o Service, converte e responde:
/// sem SQL, sem regra e sem try/catch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PedidoController(IPedidoService pedidoService) : ControllerBase
{
    /// <summary>
    /// POST /api/Pedido (C04): finaliza a compra do usuário logado. 201 com o pedido criado.
    /// O Location aponta para /api/Pedido/{id}, a rota do detalhe do pedido (C06).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPedidoResponseApiModel>>> Inserir(
        InserirPedidoRequestApiModel request)
    {
        var criado = await pedidoService.CriarAsync(request.ParaModel());
        return Created($"/api/Pedido/{criado.Pedido.Id}",
            new RetroGamesResponseApiModel<ListarPedidoResponseApiModel>(criado.ParaResponse()));
    }

    /// <summary>GET /api/Pedido/meus (C05): os pedidos do usuário logado, do mais recente para o mais antigo.</summary>
    [HttpGet("meus")]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarPedidoResponseApiModel>>>> ListarMeus()
    {
        var pedidos = await pedidoService.ListarMeusAsync();
        return Ok(new RetroGamesResponseApiModel<List<ListarPedidoResponseApiModel>>(
            pedidos.Select(pedido => pedido.ParaResponse()).ToList()));
    }

    /// <summary>
    /// GET /api/Pedido/{id} (C06): o detalhe de um pedido. O dono ou o administrador;
    /// pedido de outro cliente 403 (código 113), inexistente 404 (código 109).
    /// O {id:int} separa esta rota de /meus.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPedidoResponseApiModel>>> ListarPorId(int id)
    {
        var detalhe = await pedidoService.ObterAsync(id);
        return Ok(new RetroGamesResponseApiModel<ListarPedidoResponseApiModel>(detalhe.ParaResponse()));
    }

    /// <summary>GET /api/Pedido?idSituacao= (C07): todos os pedidos, do mais recente ao mais antigo, com filtro opcional.</summary>
    [HttpGet]
    [Authorize(Policy = Politicas.Administrador)]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarTodosPedidoResponseApiModel>>>> Listar(
        [FromQuery] ListarPedidoRequestApiModel request)
    {
        var pedidos = await pedidoService.ListarAsync(request.ParaFiltro());
        return Ok(new RetroGamesResponseApiModel<List<ListarTodosPedidoResponseApiModel>>(
            pedidos.Select(pedido => pedido.ParaListarTodosResponse()).ToList()));
    }

    /// <summary>PUT /api/Pedido/{id}/situacao (C07): muda a situação. 200 com o pedido atualizado (nunca 204).</summary>
    [HttpPut("{id:int}/situacao")]
    [Authorize(Policy = Politicas.Administrador)]
    public async Task<ActionResult<RetroGamesResponseApiModel<ListarPedidoResponseApiModel>>> AlterarSituacao(
        int id, AlterarSituacaoPedidoRequestApiModel request)
    {
        var atualizado = await pedidoService.AlterarSituacaoAsync(id, request.IdSituacao);
        return Ok(new RetroGamesResponseApiModel<ListarPedidoResponseApiModel>(atualizado.ParaResponse()));
    }
}
