using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.ActionFilters;

/// <summary>
/// Roda antes de toda action, depois do model binding. Para cada argumento que tem
/// um validator registrado (IValidator&lt;T&gt;), valida o formato. Com erro, responde
/// 400 com MODEL_STATE_INVALIDO e os erros por campo em data, e a action não roda.
/// O validator só confere formato: regra de negócio continua no Service.
/// </summary>
public class FluentValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate proximo)
    {
        // Proteção extra: a action espera um corpo e ele não veio.
        foreach (var parametro in context.ActionDescriptor.Parameters)
        {
            var vemDoCorpo = parametro.BindingInfo?.BindingSource == BindingSource.Body;
            var veio = context.ActionArguments.TryGetValue(parametro.Name, out var valor) && valor != null;
            if (vemDoCorpo && !veio)
            {
                context.Result = new BadRequestObjectResult(new RetroGamesResponseApiModel(
                    ResponseCodes.MODEL_NULL.Code, ResponseCodes.MODEL_NULL.DefaultMessage));
                return;
            }
        }

        // Erros de todos os argumentos, por campo, com o nome em camelCase (como no JSON).
        var erros = new Dictionary<string, string[]>();
        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento == null)
            {
                continue;
            }

            // Procura o validator do tipo do argumento; tipo sem validator (ex.: o id da rota) é ignorado.
            var tipoDoValidator = typeof(IValidator<>).MakeGenericType(argumento.GetType());
            if (serviceProvider.GetService(tipoDoValidator) is not IValidator validator)
            {
                continue;
            }

            var resultado = await validator.ValidateAsync(new ValidationContext<object>(argumento));
            foreach (var grupo in resultado.Errors.GroupBy(erro => erro.PropertyName))
            {
                erros[JsonNamingPolicy.CamelCase.ConvertName(grupo.Key)] =
                    grupo.Select(erro => erro.ErrorMessage).ToArray();
            }
        }

        if (erros.Count > 0)
        {
            context.Result = new BadRequestObjectResult(new RetroGamesResponseApiModel<Dictionary<string, string[]>>
            {
                ResponseCode = ResponseCodes.MODEL_STATE_INVALIDO.Code,
                Data = erros,
                ErrorMessage = ResponseCodes.MODEL_STATE_INVALIDO.DefaultMessage,
            });
            return;
        }

        await proximo();
    }
}
