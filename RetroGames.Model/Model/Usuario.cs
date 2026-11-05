namespace RetroGames.Model.Model;

/// <summary>Tabela tb_usuario.</summary>
public class Usuario
{
    /// <summary>Coluna co_seq_usuario.</summary>
    public int Id { get; set; }

    /// <summary>Coluna no_usuario.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Coluna ds_email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Coluna ds_senha_hash (hash gerado pelo PasswordHasher, nunca a senha).</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Coluna st_ativo.</summary>
    public bool Ativo { get; set; }

    /// <summary>Coluna dt_cadastro (data e hora em UTC).</summary>
    public DateTime Cadastro { get; set; }

    /// <summary>Coluna co_perfil (chave estrangeira para tb_perfil).</summary>
    public int IdPerfil { get; set; }
}
