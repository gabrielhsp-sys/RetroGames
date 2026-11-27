namespace RetroGames.Model.Filtros;

/// <summary>
/// Filtro da gestão de pedidos (C07). A API monta com o que veio na query string;
/// o PedidoService confere se a situação existe; o PedidoRepository usa no SQL.
/// Não é tabela: por isso fica fora da pasta Model.
/// </summary>
public class FiltroPedido
{
    /// <summary>Só pedidos nesta situação. Nulo: todos os pedidos.</summary>
    public int? IdSituacaoPedido { get; set; }
}
