using System.Net;

namespace RetroGames.Services;

/// <summary>
/// Catálogo dos erros de negócio. Cada erro é definido uma vez só, como
/// (Code, DefaultMessage, Status): o código a partir de 100, a mensagem padrão
/// e o status HTTP com que a API vai responder. O Service não conhece HTTP:
/// o status aqui é só um valor, quem responde é a API (aula 4).
/// </summary>
public static class BusinessErrors
{
    /// <summary>Plataforma pedida ou do jogo não existe (RN05).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) PLATAFORMA_NAO_ENCONTRADA =
        (100, "Plataforma não encontrada.", HttpStatusCode.NotFound);

    /// <summary>Nome de plataforma já usado (RN01).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) PLATAFORMA_NOME_DUPLICADO =
        (101, "Já existe uma plataforma com este nome.", HttpStatusCode.Conflict);

    /// <summary>Plataforma com jogos não pode ser excluída (RN02).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) PLATAFORMA_POSSUI_JOGOS =
        (102, "Não é possível excluir: existem jogos desta plataforma.", HttpStatusCode.Conflict);

    /// <summary>O id do jogo não existe.</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) JOGO_NAO_ENCONTRADO =
        (103, "Jogo não encontrado.", HttpStatusCode.NotFound);

    /// <summary>Condição do jogo não existe (RN05).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) CONDICAO_NAO_ENCONTRADA =
        (104, "A condição informada não existe.", HttpStatusCode.NotFound);

    /// <summary>Gênero do jogo não existe (RN05).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) GENERO_NAO_ENCONTRADO =
        (105, "Um ou mais gêneros informados não existem.", HttpStatusCode.NotFound);

    // Módulo de Compras (desafio): códigos 106 a 113, todos aqui para o grupo usar um só catálogo.
    // A versão básica (C01, C04, C05) usa 106, 110 e 111.

    /// <summary>E-mail já usado por outra conta (C01).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) EMAIL_JA_CADASTRADO =
        (106, "Já existe uma conta com este e-mail.", HttpStatusCode.Conflict);

    /// <summary>Nome de gênero já usado (C08).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) GENERO_NOME_DUPLICADO =
        (107, "Já existe um gênero com este nome.", HttpStatusCode.Conflict);

    /// <summary>Gênero com jogos não pode ser excluído (C08).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) GENERO_POSSUI_JOGOS =
        (108, "Não é possível excluir: existem jogos com este gênero.", HttpStatusCode.Conflict);

    /// <summary>O id do pedido não existe (C06 e C07).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) PEDIDO_NAO_ENCONTRADO =
        (109, "Pedido não encontrado.", HttpStatusCode.NotFound);

    /// <summary>Jogo do pedido inexistente ou inativo (C04).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) JOGO_INDISPONIVEL =
        (110, "Um ou mais jogos não estão disponíveis para venda.", HttpStatusCode.BadRequest);

    /// <summary>Quantidade pedida maior que o estoque do jogo (C04).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) ESTOQUE_INSUFICIENTE =
        (111, "Estoque insuficiente.", HttpStatusCode.Conflict);

    /// <summary>Situação de pedido inexistente (C07).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) SITUACAO_NAO_ENCONTRADA =
        (112, "Situação de pedido não encontrada.", HttpStatusCode.NotFound);

    /// <summary>Cliente pediu o pedido de outro cliente (C06).</summary>
    public static readonly (int Code, string DefaultMessage, HttpStatusCode Status) PEDIDO_DE_OUTRO_USUARIO =
        (113, "Você não tem acesso a este pedido.", HttpStatusCode.Forbidden);
}
