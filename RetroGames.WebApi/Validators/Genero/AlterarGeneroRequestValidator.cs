using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators.Genero;

/// <summary>Formato de AlterarGeneroRequestApiModel. Nome repetido é regra de negócio (Service).</summary>
public class AlterarGeneroRequestValidator : AbstractValidator<AlterarGeneroRequestApiModel>
{
    public AlterarGeneroRequestValidator()
    {
        // 50: tamanho de no_genero (VARCHAR(50)) no 01-schema.sql.
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome.")
            .MaximumLength(50).WithMessage("O nome deve ter no máximo 50 caracteres.");
    }
}
