using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Extensions;
using RetroGames.WebApi.Mappers;

namespace RetroGames.WebApi.Controllers;

/// <summary>
/// Login, cadastro de cliente, logout e o usuário da sessão. Quem confere e-mail e senha é o AutenticacaoService;
/// aqui só se cria ou apaga o cookie retrogames.sessao.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AutenticacaoController(IAutenticacaoService autenticacaoService) : ControllerBase
{
    /// <summary>POST /api/Autenticacao/login: 200 com o usuário e o cookie de sessão, ou 401 com código 6.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>>> Login(
        LoginRequestApiModel request)
    {
        var usuario = await autenticacaoService.AutenticarAsync(request.Email, request.Senha);
        if (usuario == null)
        {
            // Mesma resposta para e-mail inexistente, senha errada e usuário inativo.
            return Unauthorized(new RetroGamesResponseApiModel(
                ResponseCodes.CREDENCIAIS_INVALIDAS.Code, ResponseCodes.CREDENCIAIS_INVALIDAS.DefaultMessage));
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usuario.ParaClaimsPrincipal());
        return Ok(new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>(usuario.ParaUsuarioLogadoResponse()));
    }

    /// <summary>
    /// POST /api/Autenticacao/cadastrar (C01): cria o cliente e já inicia a sessão, como o login.
    /// 201 com o usuário e Location em /api/Autenticacao/eu; e-mail repetido, 409 com código 106.
    /// </summary>
    [HttpPost("cadastrar")]
    [AllowAnonymous]
    public async Task<ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>>> Cadastrar(
        CadastrarUsuarioRequestApiModel request)
    {
        var usuario = await autenticacaoService.CadastrarAsync(request.ParaModel(), request.Senha);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usuario.ParaClaimsPrincipal());
        return CreatedAtAction(nameof(Eu), null,
            new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>(usuario.ParaUsuarioLogadoResponse()));
    }

    /// <summary>POST /api/Autenticacao/logout: apaga o cookie de sessão.</summary>
    [HttpPost("logout")] // exige login pela política padrão
    public async Task<ActionResult<RetroGamesResponseApiModel>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new RetroGamesResponseApiModel(ResponseCodes.SUCESSO.Code, null));
    }

    /// <summary>GET /api/Autenticacao/eu: o usuário da sessão, lido do cookie.</summary>
    [HttpGet("eu")] // exige login pela política padrão
    public ActionResult<RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>> Eu()
    {
        return Ok(new RetroGamesResponseApiModel<UsuarioLogadoResponseApiModel>(User.ParaUsuarioLogadoResponse()));
    }
}
