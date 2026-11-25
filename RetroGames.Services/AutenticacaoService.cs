using Microsoft.AspNetCore.Identity;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Login e administrador inicial. A senha nunca é gravada nem registrada em log:
/// o PasswordHasher gera o hash (PBKDF2 com sal) e confere a senha digitada contra ele.
/// </summary>
public class AutenticacaoService(IRepositorySet repositorio, IPasswordHasher<Usuario> hasher) : IAutenticacaoService
{
    public async Task<Usuario?> AutenticarAsync(string email, string senha)
    {
        var usuario = await repositorio.Usuario.BuscarPorEmailAsync(email.Trim());
        if (usuario == null || !usuario.Ativo)
        {
            return null;
        }

        var resultado = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        return resultado == PasswordVerificationResult.Failed ? null : usuario;
    }

    public async Task GarantirAdministradorInicialAsync(string email, string senha)
    {
        if (await repositorio.Usuario.ExisteAdministradorAsync())
        {
            return;
        }

        var administrador = new Usuario
        {
            Nome = "Administrador",
            Email = email.Trim(),
            Ativo = true,
            Cadastro = DateTime.UtcNow,
            IdPerfil = PerfilIds.Administrador,
        };
        administrador.SenhaHash = hasher.HashPassword(administrador, senha);
        await repositorio.Usuario.InserirAsync(administrador);
    }
}
