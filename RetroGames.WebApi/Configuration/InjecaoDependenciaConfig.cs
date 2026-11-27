using Microsoft.AspNetCore.Identity;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.Configuration;

/// <summary>
/// Registro das dependências no contêiner do ASP.NET Core: "quem pedir esta
/// interface recebe esta classe". O Program.cs chama os dois métodos.
/// </summary>
public static class InjecaoDependenciaConfig
{
    /// <summary>
    /// RepositoryConfig uma vez só (singleton): só guarda a connection string.
    /// Cada Repository e o RepositorySet, um por requisição (scoped).
    /// </summary>
    public static IServiceCollection AddRepositorios(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new RepositoryConfig { ConnectionString = connectionString });

        services.AddScoped<IPlataformaRepository, PlataformaRepository>();
        services.AddScoped<IJogoRepository, JogoRepository>();
        services.AddScoped<IJogoGeneroRepository, JogoGeneroRepository>();
        services.AddScoped<IGeneroRepository, GeneroRepository>();
        services.AddScoped<ICondicaoJogoRepository, CondicaoJogoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IItemPedidoRepository, ItemPedidoRepository>();
        services.AddScoped<ISituacaoPedidoRepository, SituacaoPedidoRepository>();
        services.AddScoped<IRepositorySet, RepositorySet>();

        return services;
    }

    /// <summary>Cada Service com a sua interface, um por requisição (scoped).</summary>
    public static IServiceCollection AddServicosDeNegocio(this IServiceCollection services)
    {
        services.AddScoped<IPlataformaService, PlataformaService>();
        services.AddScoped<IGeneroService, GeneroService>();
        services.AddScoped<ICondicaoJogoService, CondicaoJogoService>();
        services.AddScoped<IJogoService, JogoService>();
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<ISituacaoPedidoService, SituacaoPedidoService>();

        // Gera e confere o hash das senhas (PBKDF2 com sal). Não guarda estado: uma instância basta.
        services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

        return services;
    }
}
