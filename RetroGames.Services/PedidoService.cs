using System.Transactions;
using RetroGames.Model.Constantes;
using RetroGames.Model.Filtros;
using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Regras de negócio de pedidos. Cada Repository lê uma tabela; aqui os dados são
/// combinados com LINQ no PedidoDetalheServiceModel. Não conhece SQL nem HTTP:
/// o usuário logado chega pelo IUsuarioAtualService.
/// </summary>
public class PedidoService(IRepositorySet repositorio, IUsuarioAtualService usuarioAtual) : IPedidoService
{
    public async Task<PedidoDetalheServiceModel> CriarAsync(List<ItemPedido> itens)
    {
        // A rota exige login (política padrão); sem id aqui é erro de configuração, não do cliente.
        var idUsuario = usuarioAtual.Id
            ?? throw new InvalidOperationException("Finalizar compra exige um usuário logado.");

        var jogos = await repositorio.Jogo.ListarPorIdsAsync(
            itens.Select(item => item.IdJogo).Distinct().ToList());

        // Jogo que não existe ou está inativo não é vendido.
        if (itens.Any(item => !jogos.Any(jogo => jogo.Id == item.IdJogo && jogo.Ativo)))
        {
            throw new BusinessRuleException(BusinessErrors.JOGO_INDISPONIVEL);
        }

        // Conferência antes de abrir a transação: com estoque insuficiente nada é gravado.
        if (itens.Any(item => jogos.First(jogo => jogo.Id == item.IdJogo).Estoque < item.Quantidade))
        {
            throw new BusinessRuleException(BusinessErrors.ESTOQUE_INSUFICIENTE);
        }

        // O preço vem sempre do banco, nunca do front.
        foreach (var item in itens)
        {
            item.ValorUnitario = jogos.First(jogo => jogo.Id == item.IdJogo).Preco;
        }

        var pedido = new Pedido
        {
            Data = DateTime.UtcNow,
            Total = itens.Sum(item => item.Quantidade * item.ValorUnitario),
            IdUsuario = idUsuario,
            IdSituacaoPedido = SituacaoPedidoIds.Pendente,
        };

        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout,
        };
        using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                   transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
        {
            // A baixa só acontece se ainda houver estoque (outra compra pode ter levado
            // o jogo depois da conferência). Se uma falhar, a exceção sai sem Complete()
            // e a transação desfaz as baixas já feitas: nada fica gravado.
            foreach (var item in itens)
            {
                if (!await repositorio.Jogo.BaixarEstoqueAsync(item.IdJogo, item.Quantidade))
                {
                    throw new BusinessRuleException(BusinessErrors.ESTOQUE_INSUFICIENTE);
                }
            }

            pedido.Id = await repositorio.Pedido.InserirAsync(pedido);
            foreach (var item in itens)
            {
                item.IdPedido = pedido.Id;
            }
            await repositorio.ItemPedido.InserirAsync(pedido.Id, itens);

            transactionScope.Complete();
        }

        var situacoes = await repositorio.SituacaoPedido.ListarAsync();
        return MontarDetalhes(new List<Pedido> { pedido }, itens, jogos, situacoes)[0];
    }

    public async Task<List<PedidoDetalheServiceModel>> ListarMeusAsync()
    {
        var idUsuario = usuarioAtual.Id
            ?? throw new InvalidOperationException("Meus pedidos exige um usuário logado.");

        // Já vêm do SQL só os do usuário, do mais recente para o mais antigo.
        var pedidos = await repositorio.Pedido.ListarPorUsuarioAsync(idUsuario);
        if (pedidos.Count == 0)
        {
            return new List<PedidoDetalheServiceModel>();
        }

        // Sempre as mesmas quatro consultas, qualquer que seja o número de pedidos.
        return await DetalharAsync(pedidos);
    }

    public async Task<PedidoDetalheServiceModel> ObterAsync(int id)
    {
        var pedido = await repositorio.Pedido.BuscarPorIdAsync(id)
            ?? throw new BusinessRuleException(BusinessErrors.PEDIDO_NAO_ENCONTRADO);

        // O cliente só vê os próprios pedidos; o administrador vê qualquer um.
        // A checagem é depois da busca: pedido inexistente é 404 para todos.
        if (!usuarioAtual.EhAdministrador && pedido.IdUsuario != usuarioAtual.Id)
        {
            throw new BusinessRuleException(BusinessErrors.PEDIDO_DE_OUTRO_USUARIO);
        }

        return (await DetalharAsync(new List<Pedido> { pedido }))[0];
    }

    public async Task<List<PedidoDetalheServiceModel>> ListarAsync(FiltroPedido filtro)
    {
        if (filtro.IdSituacaoPedido.HasValue)
        {
            var situacoes = await repositorio.SituacaoPedido.ListarAsync();
            if (!situacoes.Any(situacao => situacao.Id == filtro.IdSituacaoPedido))
            {
                throw new BusinessRuleException(BusinessErrors.SITUACAO_NAO_ENCONTRADA);
            }
        }

        var pedidos = await repositorio.Pedido.ListarAsync(filtro);
        if (pedidos.Count == 0)
        {
            return new List<PedidoDetalheServiceModel>();
        }

        var detalhes = await DetalharAsync(pedidos);
        // Nome do cliente de cada pedido: uma consulta para todos, combinada aqui.
        var usuarios = await repositorio.Usuario.ListarPorIdsAsync(
            pedidos.Select(pedido => pedido.IdUsuario).Distinct().ToList());
        foreach (var detalhe in detalhes)
        {
            detalhe.NomeUsuario = usuarios
                .FirstOrDefault(usuario => usuario.Id == detalhe.Pedido.IdUsuario)?.Nome ?? string.Empty;
        }
        return detalhes;
    }

    public async Task<PedidoDetalheServiceModel> AlterarSituacaoAsync(int id, int idSituacaoPedido)
    {
        var pedido = await repositorio.Pedido.BuscarPorIdAsync(id)
            ?? throw new BusinessRuleException(BusinessErrors.PEDIDO_NAO_ENCONTRADO);

        var situacoes = await repositorio.SituacaoPedido.ListarAsync();
        if (!situacoes.Any(situacao => situacao.Id == idSituacaoPedido))
        {
            throw new BusinessRuleException(BusinessErrors.SITUACAO_NAO_ENCONTRADA);
        }

        // Um só comando: sem transação. O desafio não define regra de transição
        // (qualquer situação vale) nem devolução de estoque ao cancelar.
        await repositorio.Pedido.AlterarSituacaoAsync(id, idSituacaoPedido);
        pedido.IdSituacaoPedido = idSituacaoPedido;

        return (await DetalharAsync(new List<Pedido> { pedido }))[0];
    }

    /// <summary>
    /// Busca os itens, os jogos e as situações de uma lista de pedidos (três consultas,
    /// qualquer que seja o número de pedidos) e monta os detalhes, na ordem recebida.
    /// </summary>
    private async Task<List<PedidoDetalheServiceModel>> DetalharAsync(List<Pedido> pedidos)
    {
        var itens = await repositorio.ItemPedido.ListarPorPedidosAsync(
            pedidos.Select(pedido => pedido.Id).ToList());
        var jogos = await repositorio.Jogo.ListarPorIdsAsync(
            itens.Select(item => item.IdJogo).Distinct().ToList());
        var situacoes = await repositorio.SituacaoPedido.ListarAsync();

        return MontarDetalhes(pedidos, itens, jogos, situacoes);
    }

    /// <summary>
    /// Combina pedidos, itens, jogos e situações com LINQ, mantendo a ordem dos pedidos
    /// recebidos. Só monta: as consultas já foram feitas por quem chama.
    /// </summary>
    private static List<PedidoDetalheServiceModel> MontarDetalhes(
        List<Pedido> pedidos, List<ItemPedido> itens, List<Jogo> jogos, List<SituacaoPedido> situacoes)
    {
        var detalhes = from pedido in pedidos
                       join situacao in situacoes on pedido.IdSituacaoPedido equals situacao.Id
                       select new PedidoDetalheServiceModel
                       {
                           Pedido = pedido,
                           DescricaoSituacao = situacao.Descricao,
                           Itens = (from item in itens
                                    where item.IdPedido == pedido.Id
                                    join jogo in jogos on item.IdJogo equals jogo.Id
                                    select new ItemPedidoDetalheServiceModel
                                    {
                                        Item = item,
                                        NomeJogo = jogo.Nome,
                                    }).ToList(),
                       };
        return detalhes.ToList();
    }
}
