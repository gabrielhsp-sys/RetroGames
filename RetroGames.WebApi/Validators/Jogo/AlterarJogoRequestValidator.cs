using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators.Jogo;

/// <summary>
/// Formato de AlterarJogoRequestApiModel (RN06). Se a plataforma, a condição e os
/// gêneros existem é regra de negócio (RN05): o Service confere e responde 404.
/// </summary>
public class AlterarJogoRequestValidator : AbstractValidator<AlterarJogoRequestApiModel>
{
    public AlterarJogoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");
        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");
        RuleFor(x => x.AnoLancamento)
            .InclusiveBetween(1970, 2100).WithMessage("Ano de lançamento inválido.");
        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.")
            .PrecisionScale(10, 2, true).WithMessage("O preço deve ter no máximo 2 casas decimais.");
        RuleFor(x => x.Estoque)
            .GreaterThanOrEqualTo(0).WithMessage("O estoque não pode ser negativo.");
        RuleFor(x => x.IdPlataforma)
            .GreaterThan(0).WithMessage("Informe a plataforma.");
        RuleFor(x => x.IdCondicaoJogo)
            .GreaterThan(0).WithMessage("Informe a condição.");
        RuleFor(x => x.IdsGeneros)
            .NotEmpty().WithMessage("Informe ao menos um gênero.");
    }
}
