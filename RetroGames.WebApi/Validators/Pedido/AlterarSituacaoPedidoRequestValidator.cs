using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators.Pedido;

/// <summary>
/// Formato de AlterarSituacaoPedidoRequestApiModel. Se a situação existe é regra
/// de negócio (Service, 404 código 112): o validator não consulta o banco.
/// </summary>
public class AlterarSituacaoPedidoRequestValidator : AbstractValidator<AlterarSituacaoPedidoRequestApiModel>
{
    public AlterarSituacaoPedidoRequestValidator()
    {
        RuleFor(x => x.IdSituacao)
            .GreaterThan(0).WithMessage("Informe a situação do pedido.");
    }
}
