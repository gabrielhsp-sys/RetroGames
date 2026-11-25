using System.Transactions;
using RetroGames.Model.Filtros;
using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Catálogo e regras de negócio de jogos. Cada Repository lê uma tabela; aqui os
/// dados são combinados com LINQ no JogoDetalheServiceModel. Não conhece SQL nem HTTP:
/// quem chama chega pelo IUsuarioAtualService.
/// </summary>
public class JogoService(IRepositorySet repositorio, IUsuarioAtualService usuarioAtual) : IJogoService
{
    public async Task<(List<JogoDetalheServiceModel> Itens, int Total)> ListarAsync(FiltroJogo filtro)
    {
        // RN04: inativos só para o administrador. Para os demais, o pedido é ignorado.
        filtro.IncluirInativos = filtro.IncluirInativos && usuarioAtual.EhAdministrador;

        // Filtro por gênero depende de outra tabela: os ids dos jogos do gênero são
        // selecionados com LINQ e vão para o SQL, que mantém contagem e paginação.
        filtro.Ids = null;
        if (filtro.IdGenero != null)
        {
            var vinculos = await repositorio.JogoGenero.ListarAsync();
            filtro.Ids = vinculos
                .Where(vinculo => vinculo.IdGenero == filtro.IdGenero)
                .Select(vinculo => vinculo.IdJogo)
                .ToList();
        }

        var (jogos, total) = await repositorio.Jogo.ListarAsync(filtro);

        return (await MontarDetalhesAsync(jogos), total);
    }

    public async Task<JogoDetalheServiceModel> BuscarPorIdAsync(int id)
    {
        var jogo = await repositorio.Jogo.BuscarPorIdAsync(id);
        // RN04: para quem não é administrador, jogo inativo é como jogo que não existe.
        if (jogo == null || (!jogo.Ativo && !usuarioAtual.EhAdministrador))
        {
            throw new BusinessRuleException(BusinessErrors.JOGO_NAO_ENCONTRADO);
        }

        var detalhes = await MontarDetalhesAsync(new List<Jogo> { jogo });
        return detalhes[0];
    }

    public async Task<Jogo> InserirAsync(Jogo jogo, List<int> idsGenero)
    {
        jogo.Nome = jogo.Nome.Trim();
        jogo.Ativo = true;
        jogo.Cadastro = DateTime.UtcNow;
        // Gênero repetido na lista viraria chave duplicada em tb_jogo_genero.
        idsGenero = idsGenero.Distinct().ToList();
        await ConferirReferenciasAsync(jogo, idsGenero);

        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout,
        };
        using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                   transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
        {
            jogo.Id = await repositorio.Jogo.InserirAsync(jogo);
            await repositorio.JogoGenero.InserirAsync(jogo.Id, idsGenero);
            transactionScope.Complete();
        }
        return jogo;
    }

    public async Task AlterarAsync(Jogo jogo, List<int> idsGenero)
    {
        if (await repositorio.Jogo.BuscarPorIdAsync(jogo.Id) == null)
        {
            throw new BusinessRuleException(BusinessErrors.JOGO_NAO_ENCONTRADO);
        }

        jogo.Nome = jogo.Nome.Trim();
        idsGenero = idsGenero.Distinct().ToList();
        await ConferirReferenciasAsync(jogo, idsGenero);

        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout,
        };
        using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                   transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
        {
            await repositorio.Jogo.AlterarAsync(jogo);
            await repositorio.JogoGenero.RemoverPorJogoAsync(jogo.Id);
            await repositorio.JogoGenero.InserirAsync(jogo.Id, idsGenero);
            transactionScope.Complete();
        }
    }

    public async Task DesativarAsync(int id)
    {
        if (await repositorio.Jogo.BuscarPorIdAsync(id) == null)
        {
            throw new BusinessRuleException(BusinessErrors.JOGO_NAO_ENCONTRADO);
        }

        await repositorio.Jogo.DesativarAsync(id); // RN03: só marca inativo, nunca apaga
    }

    /// <summary>RN05: plataforma, condição e cada gênero informados precisam existir.</summary>
    private async Task ConferirReferenciasAsync(Jogo jogo, List<int> idsGenero)
    {
        if (await repositorio.Plataforma.BuscarPorIdAsync(jogo.IdPlataforma) == null)
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);
        }

        var condicoes = await repositorio.CondicaoJogo.ListarAsync();
        if (!condicoes.Any(condicao => condicao.Id == jogo.IdCondicaoJogo))
        {
            throw new BusinessRuleException(BusinessErrors.CONDICAO_NAO_ENCONTRADA);
        }

        var generos = await repositorio.Genero.ListarAsync();
        if (idsGenero.Except(generos.Select(genero => genero.Id)).Any())
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NAO_ENCONTRADO);
        }
    }

    /// <summary>
    /// Monta o detalhe de uma lista de jogos com LINQ. Faz sempre as mesmas quatro
    /// consultas (plataformas, condições, gêneros e vínculos dos jogos da lista),
    /// qualquer que seja o tamanho da lista: nunca uma consulta por jogo.
    /// </summary>
    private async Task<List<JogoDetalheServiceModel>> MontarDetalhesAsync(List<Jogo> jogos)
    {
        // Página vazia: não há o que montar.
        if (jogos.Count == 0)
        {
            return new List<JogoDetalheServiceModel>();
        }

        var plataformas = await repositorio.Plataforma.ListarAsync();
        var condicoes = await repositorio.CondicaoJogo.ListarAsync();
        var generos = await repositorio.Genero.ListarAsync();
        var vinculos = await repositorio.JogoGenero.ListarPorJogosAsync(
            jogos.Select(jogo => jogo.Id).ToList());

        var detalhes = from jogo in jogos
                       join plataforma in plataformas on jogo.IdPlataforma equals plataforma.Id
                       join condicao in condicoes on jogo.IdCondicaoJogo equals condicao.Id
                       select new JogoDetalheServiceModel
                       {
                           Jogo = jogo,
                           NomePlataforma = plataforma.Nome,
                           DescricaoCondicao = condicao.Descricao,
                           Generos = (from vinculo in vinculos
                                      where vinculo.IdJogo == jogo.Id
                                      join genero in generos on vinculo.IdGenero equals genero.Id
                                      select genero).ToList(),
                       };
        return detalhes.ToList();
    }
}
