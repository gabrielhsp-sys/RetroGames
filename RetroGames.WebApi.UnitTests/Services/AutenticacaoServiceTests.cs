using Microsoft.AspNetCore.Identity;
using Moq;
using RetroGames.Model.Constantes;
using RetroGames.Model.Model;
using RetroGames.Repository;
using RetroGames.Services;

namespace RetroGames.WebApi.UnitTests.Services;

/// <summary>
/// Testes do cadastro de cliente (C01) e do administrador inicial no AutenticacaoService.
/// O repositório de usuários é um mock; o PasswordHasher é o real, para conferir o hash
/// gravado contra a senha digitada. Nenhum teste usa o banco.
/// </summary>
public class AutenticacaoServiceTests
{
    private const string Senha = "senha-forte-123";

    private readonly Mock<IUsuarioRepository> usuarioRepository = new();
    private readonly Mock<IRepositorySet> repositorio = new();
    private readonly PasswordHasher<Usuario> hasher = new();

    public AutenticacaoServiceTests()
    {
        repositorio.Setup(r => r.Usuario).Returns(usuarioRepository.Object);
    }

    private AutenticacaoService CriarService()
    {
        return new AutenticacaoService(repositorio.Object, hasher);
    }

    private static Usuario CriarUsuario()
    {
        return new Usuario { Nome = "Ana Souza", Email = "ana@exemplo.com" };
    }

    [Fact]
    public async Task Cadastrar_DadosValidos_GravaClienteAtivoEDevolveComId()
    {
        // Arrange
        Usuario? gravado = null;
        usuarioRepository.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(usuario => gravado = usuario)
            .ReturnsAsync(42);
        var service = CriarService();

        // Act
        var criado = await service.CadastrarAsync(CriarUsuario(), Senha);

        // Assert
        Assert.Equal(42, criado.Id);
        Assert.NotNull(gravado);
        Assert.Equal(PerfilIds.Cliente, gravado.IdPerfil);
        Assert.True(gravado.Ativo);
    }

    [Fact]
    public async Task Cadastrar_SenhaDigitada_GravaSoOHashQueConfereComASenha()
    {
        // Arrange
        Usuario? gravado = null;
        usuarioRepository.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(usuario => gravado = usuario)
            .ReturnsAsync(1);
        var service = CriarService();

        // Act
        await service.CadastrarAsync(CriarUsuario(), Senha);

        // Assert: o que vai ao banco não é a senha, mas confere com ela (RS04).
        Assert.NotNull(gravado);
        Assert.NotEqual(Senha, gravado.SenhaHash);
        Assert.Equal(PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(gravado, gravado.SenhaHash, Senha));
    }

    [Fact]
    public async Task Cadastrar_EmailComEspacosEMaiusculas_GravaSemEspacosEMinusculo()
    {
        // Arrange
        Usuario? gravado = null;
        usuarioRepository.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(usuario => gravado = usuario)
            .ReturnsAsync(1);
        var usuario = CriarUsuario();
        usuario.Email = "  Ana@Exemplo.COM ";
        var service = CriarService();

        // Act
        await service.CadastrarAsync(usuario, Senha);

        // Assert
        Assert.Equal("ana@exemplo.com", gravado!.Email);
    }

    [Fact]
    public async Task Cadastrar_EmailJaCadastrado_LancaEmailJaCadastradoENaoGrava()
    {
        // Arrange
        usuarioRepository.Setup(r => r.BuscarPorEmailAsync("ana@exemplo.com"))
            .ReturnsAsync(new Usuario { Id = 7, Email = "ana@exemplo.com" });
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarAsync(CriarUsuario(), Senha));

        // Assert
        Assert.Equal(BusinessErrors.EMAIL_JA_CADASTRADO, excecao.Erro);
        usuarioRepository.Verify(r => r.InserirAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Cadastrar_BancoRecusaEmailDuplicado_LancaEmailJaCadastrado()
    {
        // Arrange: a conferência passa, mas outro cadastro gravou o e-mail antes (UNIQUE, erro 1062).
        usuarioRepository.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ThrowsAsync(new RegistroDuplicadoException("Duplicate entry", new Exception()));
        var service = CriarService();

        // Act
        var excecao = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CadastrarAsync(CriarUsuario(), Senha));

        // Assert
        Assert.Equal(BusinessErrors.EMAIL_JA_CADASTRADO, excecao.Erro);
    }

    [Fact]
    public async Task GarantirAdministradorInicial_EmailJaUsadoPorOutraConta_NaoDerrubaAInicializacao()
    {
        // Arrange: não há administrador, mas o e-mail do .env já é de um cliente.
        usuarioRepository.Setup(r => r.ExisteAdministradorAsync()).ReturnsAsync(false);
        usuarioRepository.Setup(r => r.InserirAsync(It.IsAny<Usuario>()))
            .ThrowsAsync(new RegistroDuplicadoException("Duplicate entry", new Exception()));
        var service = CriarService();

        // Act
        var excecao = await Record.ExceptionAsync(
            () => service.GarantirAdministradorInicialAsync("admin@exemplo.com", Senha));

        // Assert
        Assert.Null(excecao);
    }
}
