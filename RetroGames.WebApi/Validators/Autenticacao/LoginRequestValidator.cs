using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators.Autenticacao;

/// <summary>Formato do login. Se o e-mail existe e a senha confere é o AutenticacaoService que diz.</summary>
public class LoginRequestValidator : AbstractValidator<LoginRequestApiModel>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(256).WithMessage("O e-mail deve ter no máximo 256 caracteres.");
        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Informe a senha.");
    }
}
