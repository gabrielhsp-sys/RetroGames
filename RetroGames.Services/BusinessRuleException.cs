using System.Net;

namespace RetroGames.Services;

/// <summary>
/// Única exceção de negócio. Lançada pelo Service quando uma regra é violada,
/// sempre com um erro do catálogo: throw new BusinessRuleException(BusinessErrors.X).
/// A mensagem da exceção é a mensagem padrão do erro.
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>O erro do catálogo BusinessErrors: código, mensagem e status.</summary>
    public (int Code, string DefaultMessage, HttpStatusCode Status) Erro { get; }

    public BusinessRuleException((int Code, string DefaultMessage, HttpStatusCode Status) erro)
        : base(erro.DefaultMessage)
    {
        Erro = erro;
    }
}
