using RetroGames.Model.Model;

namespace RetroGames.Repository;

/// <summary>Todo o SQL da tabela tb_situacao_pedido. Só executa SQL e devolve dados.</summary>
public class SituacaoPedidoRepository(RepositoryConfig config) : RepositoryBase(config), ISituacaoPedidoRepository
{
    public async Task<List<SituacaoPedido>> ListarAsync()
    {
        await using var conexao = await CriarConexaoAsync();
        await using var comando = CriarComando(conexao,
            "SELECT co_situacao_pedido, ds_situacao_pedido FROM tb_situacao_pedido ORDER BY co_situacao_pedido");
        await using var leitor = await comando.ExecuteReaderAsync();

        var situacoes = new List<SituacaoPedido>();
        while (await leitor.ReadAsync())
        {
            situacoes.Add(new SituacaoPedido
            {
                Id = leitor.GetInt32("co_situacao_pedido"),
                Descricao = leitor.GetString("ds_situacao_pedido"),
            });
        }
        return situacoes;
    }
}
