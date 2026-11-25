using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RetroGames.Services;
using RetroGames.WebApi.ActionFilters;
using RetroGames.WebApi.ApiModels;
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
// Todos os validators deste projeto (IValidator<T>) e o filtro que os aplica antes de cada action.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers(opcoes => opcoes.Filters.Add<FluentValidationFilter>())
    .ConfigureApiBehaviorOptions(opcoes =>
    {
        // O [ApiController] confere o model binding antes do FluentValidationFilter.
        // Corpo ausente numa action que espera um: 400 com código 2 (MODEL_NULL).
        // JSON quebrado ou tipo errado (ex.: ?pagina=abc): 400 com código 3, sem lista de campos.
        opcoes.InvalidModelStateResponseFactory = contexto =>
        {
            var esperaCorpo = contexto.ActionDescriptor.Parameters
                .Any(parametro => parametro.BindingInfo?.BindingSource == BindingSource.Body);
            var semCorpo = contexto.HttpContext.Request.ContentLength == 0
                || contexto.HttpContext.Features.Get<IHttpRequestBodyDetectionFeature>()?.CanHaveBody == false;
            var codigo = esperaCorpo && semCorpo ? ResponseCodes.MODEL_NULL : ResponseCodes.MODEL_STATE_INVALIDO;
            return new BadRequestObjectResult(new RetroGamesResponseApiModel(codigo.Code, codigo.DefaultMessage));
        };
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Administrador inicial (RS05): e-mail e senha vêm do .env. Se já existe um administrador, nada muda.
// A senha só passa ao Service, que grava o hash; ela nunca vai para o log.
var emailAdministrador = app.Configuration["AdminInicial:Email"];
var senhaAdministrador = app.Configuration["AdminInicial:Senha"];
if (string.IsNullOrWhiteSpace(emailAdministrador) || string.IsNullOrWhiteSpace(senhaAdministrador))
{
    throw new InvalidOperationException(
        "Administrador inicial não configurado. Confira as linhas AdminInicial__Email e " +
        "AdminInicial__Senha no arquivo .env da raiz do repositório (modelo em .env.example).");
}
using (var escopo = app.Services.CreateScope())
{
    var autenticacaoService = escopo.ServiceProvider.GetRequiredService<IAutenticacaoService>();
    await autenticacaoService.GarantirAdministradorInicialAsync(emailAdministrador, senhaAdministrador);
}

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
