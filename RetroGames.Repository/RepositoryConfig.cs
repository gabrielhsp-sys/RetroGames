namespace RetroGames.Repository;

/// <summary>Configuração do acesso ao banco. O valor chega pela API na aula 4.</summary>
public class RepositoryConfig
{
    /// <summary>Connection string do MySQL (ConnectionStrings__Default no .env).</summary>
    public string ConnectionString { get; set; } = string.Empty;
}
