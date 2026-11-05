namespace RetroGames.Repository;

/// <summary>
/// Lançada pelo método de gravação da RepositoryBase quando o MySQL recusa
/// um valor repetido numa coluna UNIQUE (erro 1062).
/// O Service a converte no erro de negócio correspondente.
/// </summary>
public class RegistroDuplicadoException : Exception
{
    public RegistroDuplicadoException(string mensagem, Exception excecaoOriginal)
        : base(mensagem, excecaoOriginal)
    {
    }
}
