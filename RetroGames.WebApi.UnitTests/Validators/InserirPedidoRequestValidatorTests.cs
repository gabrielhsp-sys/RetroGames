using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Validators.Pedido;

namespace RetroGames.WebApi.UnitTests.Validators;

/// <summary>Formato do pedido (C04): itens, quantidade de 1 a 10 e jogo sem repetição.</summary>
public class InserirPedidoRequestValidatorTests
{
    private readonly InserirPedidoRequestValidator validator = new();

    private static InserirPedidoRequestApiModel Pedido(params (int IdJogo, int Quantidade)[] itens)
    {
        return new InserirPedidoRequestApiModel
        {
            Itens = itens.Select(item => new InserirItemPedidoRequestApiModel
            {
                IdJogo = item.IdJogo,
                Quantidade = item.Quantidade,
            }).ToList(),
        };
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Validar_QuantidadeNaFaixa_EhValido(int quantidade)
    {
        var resultado = validator.Validate(Pedido((1, quantidade)));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validar_QuantidadeForaDaFaixa_EhInvalido(int quantidade)
    {
        var resultado = validator.Validate(Pedido((1, quantidade)));

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Validar_SemItens_EhInvalido()
    {
        var resultado = validator.Validate(Pedido());

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Validar_MesmoJogoDuasVezes_EhInvalido()
    {
        var resultado = validator.Validate(Pedido((1, 1), (1, 2)));

        Assert.False(resultado.IsValid);
    }
}
