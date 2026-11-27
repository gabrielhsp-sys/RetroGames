using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators.Pedido;

/// <summary>
/// Formato do pedido (C04): ao menos um item, quantidade de 1 a 10 e cada jogo uma só vez
/// (o UNIQUE de tb_item_pedido é a rede de segurança). Se o jogo existe, está ativo e tem
/// estoque é regra de negócio: o PedidoService confere (códigos 110 e 111).
/// </summary>
public class InserirPedidoRequestValidator : AbstractValidator<InserirPedidoRequestApiModel>
{
    public InserirPedidoRequestValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Informe ao menos um item.")
            .Must(itens => itens == null
                           || itens.Where(item => item != null).Select(item => item.IdJogo).Distinct().Count()
                              == itens.Count(item => item != null))
            .WithMessage("Cada jogo deve aparecer uma só vez no pedido.");
        RuleForEach(x => x.Itens)
            .NotNull().WithMessage("Item inválido.")
            .ChildRules(item =>
            {
                item.RuleFor(x => x.IdJogo)
                    .GreaterThan(0).WithMessage("Informe o jogo.");
                item.RuleFor(x => x.Quantidade)
                    .InclusiveBetween(1, 10).WithMessage("A quantidade deve ser de 1 a 10.");
            });
    }
}
