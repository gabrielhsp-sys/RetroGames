using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Middlewares;

// Lê o arquivo .env (procurando nas pastas acima até a raiz do repositório) e põe
// cada linha nas variáveis de ambiente. ConnectionStrings__Default vira
// ConnectionStrings:Default na configuração. Nenhuma senha fica neste arquivo.
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Sem a connection string a API não funciona: para aqui, na subida, com uma mensagem clara.
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ConnectionStrings:Default' não encontrada. " +
        "Confira a linha ConnectionStrings__Default no arquivo .env da raiz do repositório " +
        "(modelo em .env.example).");
}

// 1. Serviços (injeção de dependência)
builder.Services.AddRepositorios(connectionString);
builder.Services.AddServicosDeNegocio();
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. Pipeline HTTP (a ordem importa)
// Primeira etapa: qualquer exceção que suba das etapas seguintes vira envelope de erro.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Descrição da API em /openapi/v1.json e a página de testes em /swagger.
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/openapi/v1.json", "RetroGames API v1"));
}

app.MapControllers();

app.Run();
