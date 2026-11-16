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
}
