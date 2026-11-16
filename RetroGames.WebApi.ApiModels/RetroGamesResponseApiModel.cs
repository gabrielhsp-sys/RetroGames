using System.Text.Json.Serialization;

namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Envelope de toda resposta da API, sem dados: usado nas respostas de erro,
/// em que "data" é omitido. No JSON: { "responseCode", "errorMessage" }.
/// </summary>
public class RetroGamesResponseApiModel
{
    /// <summary>0 = sucesso, 1 = erro inesperado, 100 em diante = erro de negócio.</summary>
    [JsonPropertyOrder(0)]
    public int ResponseCode { get; set; }

    /// <summary>Mensagem do erro; null quando deu certo.</summary>
    [JsonPropertyOrder(2)]
    public string? ErrorMessage { get; set; }

    public RetroGamesResponseApiModel()
    {
    }

    public RetroGamesResponseApiModel(int responseCode, string? errorMessage)
    {
        ResponseCode = responseCode;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Envelope com dados, usado nas respostas de sucesso.
/// No JSON: { "responseCode": 0, "data": ..., "errorMessage": null }.
/// </summary>
public class RetroGamesResponseApiModel<T> : RetroGamesResponseApiModel
{
    /// <summary>O conteúdo da resposta: sempre um ApiModel, nunca uma classe do modelo.</summary>
    [JsonPropertyOrder(1)]
    public T? Data { get; set; }

    public RetroGamesResponseApiModel()
    {
    }

    /// <summary>Resposta de sucesso: responseCode 0 e os dados.</summary>
    public RetroGamesResponseApiModel(T data)
        : base(ResponseCodes.SUCESSO.Code, null)
    {
        Data = data;
    }
}
