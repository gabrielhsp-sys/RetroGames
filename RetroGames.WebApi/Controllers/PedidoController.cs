using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Pedidos do Módulo de Compras. Sem [AllowAnonymous]: toda action exige login pela
/// política padrão (401, código 4). Recebe, chama o Service, converte e responde:
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
}
