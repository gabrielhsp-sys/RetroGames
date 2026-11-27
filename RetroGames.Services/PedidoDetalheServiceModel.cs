using RetroGames.Model.Model;

namespace RetroGames.Services;

/// <summary>
/// Um pedido com os dados que vêm de outras tabelas, combinados com LINQ no PedidoService:
/// a descrição da situação e os itens, cada um com o nome do jogo.
/// </summary>
public class PedidoDetalheServiceModel
{
    /// <summary>O pedido, como o Repository entrega (tb_pedido).</summary>
    public Pedido Pedido { get; set; } = new();

    /// <summary>Descrição da situação do pedido (tb_situacao_pedido).</summary>
    public string DescricaoSituacao { get; set; } = string.Empty;

    /// <summary>
    /// Nome do cliente dono do pedido (tb_usuario). Só a gestão de pedidos (C07) preenche;
    /// nas demais consultas fica vazio.
    /// </summary>
    public string NomeUsuario { get; set; } = string.Empty;

    /// <summary>Itens do pedido (tb_item_pedido), com o nome do jogo (tb_jogo).</summary>
    public List<ItemPedidoDetalheServiceModel> Itens { get; set; } = new();
}
