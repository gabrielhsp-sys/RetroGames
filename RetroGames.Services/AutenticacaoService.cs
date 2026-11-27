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

    public async Task<Usuario> CadastrarAsync(Usuario usuario, string senha)
    {
        usuario.Nome = usuario.Nome.Trim();
        // E-mail sem espaços e em minúsculas: "Ana@X.com " e "ana@x.com" são a mesma conta.
        usuario.Email = usuario.Email.Trim().ToLowerInvariant();

        if (await repositorio.Usuario.BuscarPorEmailAsync(usuario.Email) != null)
        {
            throw new BusinessRuleException(BusinessErrors.EMAIL_JA_CADASTRADO);
        }

        // O cliente não escolhe perfil nem situação: todo cadastro público é Cliente ativo.
        usuario.Ativo = true;
        usuario.Cadastro = DateTime.UtcNow;
        usuario.IdPerfil = PerfilIds.Cliente;
        usuario.SenhaHash = hasher.HashPassword(usuario, senha);

        try
        {
            usuario.Id = await repositorio.Usuario.InserirAsync(usuario);
        }
        catch (RegistroDuplicadoException)
        {
            // Dois cadastros com o mesmo e-mail ao mesmo tempo: o UNIQUE recusa o segundo.
            throw new BusinessRuleException(BusinessErrors.EMAIL_JA_CADASTRADO);
        }
        return usuario;
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
        try
        {
            await repositorio.Usuario.InserirAsync(administrador);
        }
        catch (RegistroDuplicadoException)
        {
            // O e-mail já tem conta (por exemplo, um cliente se cadastrou com ele). A API sobe
            // mesmo assim, sem administrador novo: quem configura o .env escolhe outro e-mail.
        }
    }
}
