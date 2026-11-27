using FluentValidation;
using RetroGames.WebApi.ApiModels;

namespace RetroGames.WebApi.Validators.Autenticacao;

/// <summary>
/// Formato do cadastro de cliente (C01). Se o e-mail já tem conta é regra de negócio:
/// o AutenticacaoService confere e responde 409 (código 106).
/// Tamanhos de nome e e-mail: os das colunas no_usuario e ds_email (01-schema.sql).
/// Senha: nem a especificação nem o desafio definem tamanho; adotado o mínimo de 8 caracteres.
/// </summary>
public class CadastrarUsuarioRequestValidator : AbstractValidator<CadastrarUsuarioRequestApiModel>
{
    public CadastrarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(256).WithMessage("O e-mail deve ter no máximo 256 caracteres.");
        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Informe a senha.")
            .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.");
    }
}
