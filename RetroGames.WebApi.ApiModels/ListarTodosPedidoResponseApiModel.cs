namespace RetroGames.WebApi.ApiModels;

/// <summary>
/// Um pedido na gestão do administrador (GET /api/Pedido): o mesmo de Meus pedidos,
/// mais o nome do cliente. Sem e-mail nem outros dados do cliente.
/// </summary>
public class ListarTodosPedidoResponseApiModel : ListarPedidoResponseApiModel
{
    /// <summary>Nome do cliente dono do pedido (tb_usuario).</summary>
    public string NomeUsuario { get; set; } = string.Empty;
}
