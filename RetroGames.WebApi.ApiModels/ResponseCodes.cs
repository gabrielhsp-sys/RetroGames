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
}
