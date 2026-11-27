using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>Consulta das situações de pedido (C07), só para o administrador.</summary>
[ApiController]
[Route("api/[controller]")]
public class SituacaoPedidoController(ISituacaoPedidoService situacaoPedidoService) : ControllerBase
{
    /// <summary>GET /api/SituacaoPedido: todas as situações de pedido.</summary>
    [HttpGet]
    [Authorize(Policy = Politicas.Administrador)]
    public async Task<ActionResult<RetroGamesResponseApiModel<List<ListarSituacaoPedidoResponseApiModel>>>> Listar()
    {
        var situacoes = await situacaoPedidoService.ListarAsync();
        return Ok(new RetroGamesResponseApiModel<List<ListarSituacaoPedidoResponseApiModel>>(
            situacoes.Select(situacaoPedido => situacaoPedido.ParaResponse()).ToList()));
    }
}
