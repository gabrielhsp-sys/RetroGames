namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Catálogo dos códigos da própria API (0 a 99), como (Code, DefaultMessage).
/// Erros de negócio não entram aqui: eles vêm do BusinessErrors (100 em diante).
/// Código novo recebe o próximo número livre.
/// </summary>
public static class ResponseCodes
{
    /// <summary>Deu certo. Sucesso não tem mensagem: errorMessage sai null.</summary>
    public static readonly (int Code, string DefaultMessage) SUCESSO =
        (0, string.Empty);

    /// <summary>Exceção não tratada. Mensagem genérica, sem detalhes internos.</summary>
    public static readonly (int Code, string DefaultMessage) ERRO =
        (1, "Ocorreu um erro inesperado. Tente novamente mais tarde.");

    /// <summary>A action esperava um corpo (ApiModel de entrada) e ele não veio.</summary>
    public static readonly (int Code, string DefaultMessage) MODEL_NULL =
        (2, "O modelo veio nulo.");

    /// <summary>
    /// Dados mal formados: erro de validator (com os erros por campo em data)
    /// ou JSON quebrado / tipo errado (sem a lista de campos).
    /// </summary>
    public static readonly (int Code, string DefaultMessage) MODEL_STATE_INVALIDO =
        (3, "Há um erro com o modelo enviado.");

    /// <summary>Rota que exige login, chamada sem sessão (401).</summary>
    public static readonly (int Code, string DefaultMessage) NAO_AUTENTICADO =
        (4, "Faça login para continuar.");

    /// <summary>Usuário logado sem o perfil exigido pela rota (403).</summary>
    public static readonly (int Code, string DefaultMessage) ACESSO_NEGADO =
        (5, "Você não tem permissão para esta operação.");

    /// <summary>Login com e-mail inexistente, senha errada ou usuário inativo (401). Não diz qual dos três.</summary>
    public static readonly (int Code, string DefaultMessage) CREDENCIAIS_INVALIDAS =
        (6, "E-mail ou senha inválidos.");
}
