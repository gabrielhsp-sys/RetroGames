namespace RetroGames.Model.Model;

/// <summary>Tabela tb_perfil.</summary>
public class Perfil
{
    /// <summary>Coluna co_perfil (chave fixa).</summary>
    public int Id { get; set; }

    /// <summary>Coluna ds_perfil.</summary>
    public string Descricao { get; set; } = string.Empty;
}
