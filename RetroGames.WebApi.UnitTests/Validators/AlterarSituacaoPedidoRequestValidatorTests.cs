using RetroGames.WebApi.ApiModels;
using RetroGames.WebApi.Validators.Pedido;

namespace RetroGames.WebApi.UnitTests.Validators;

/// <summary>Formato da troca de situação (C07): a situação precisa ser informada (maior que zero).</summary>
public class AlterarSituacaoPedidoRequestValidatorTests
{
    private readonly AlterarSituacaoPedidoRequestValidator validator = new();

    [Fact]
    public void Validar_SituacaoInformada_EhValido()
    {
        var resultado = validator.Validate(new AlterarSituacaoPedidoRequestApiModel { IdSituacao = 2 });

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_SituacaoNaoInformada_EhInvalido(int idSituacao)
    {
        var resultado = validator.Validate(new AlterarSituacaoPedidoRequestApiModel { IdSituacao = idSituacao });

        Assert.False(resultado.IsValid);
    }
}
