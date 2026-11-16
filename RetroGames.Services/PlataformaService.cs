using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Regras de negócio de plataformas. Não conhece SQL nem HTTP: usa só o
/// IRepositorySet e lança BusinessRuleException com um erro do catálogo.
/// </summary>
public class PlataformaService(IRepositorySet repositorio) : IPlataformaService
{
    public async Task<List<Plataforma>> ListarAsync()
    {
        return await repositorio.Plataforma.ListarAsync();
    }

    public async Task<Plataforma> BuscarPorIdAsync(int id)
    {
        var plataforma = await repositorio.Plataforma.BuscarPorIdAsync(id);
        if (plataforma == null)
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);
        }
        return plataforma;
    }

    public async Task<Plataforma> InserirAsync(Plataforma plataforma)
    {
        plataforma.Nome = plataforma.Nome.Trim();
        if (await repositorio.Plataforma.ExisteComNomeAsync(plataforma.Nome))
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO); // RN01
        }

        try
        {
            plataforma.Id = await repositorio.Plataforma.InserirAsync(plataforma);
        }
        catch (RegistroDuplicadoException) // outra gravação com o mesmo nome chegou antes
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO);
        }
        return plataforma;
    }

    public async Task AlterarAsync(Plataforma plataforma)
    {
        if (await repositorio.Plataforma.BuscarPorIdAsync(plataforma.Id) == null)
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);
        }

        plataforma.Nome = plataforma.Nome.Trim();
        // Ignora a própria plataforma: manter o próprio nome não é duplicidade.
        if (await repositorio.Plataforma.ExisteComNomeAsync(plataforma.Nome, plataforma.Id))
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO); // RN01
        }

        try
        {
            await repositorio.Plataforma.AlterarAsync(plataforma);
        }
        catch (RegistroDuplicadoException) // outra gravação com o mesmo nome chegou antes
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NOME_DUPLICADO);
        }
    }

    public async Task ExcluirAsync(int id)
    {
        if (await repositorio.Plataforma.BuscarPorIdAsync(id) == null)
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_NAO_ENCONTRADA);
        }

        if (await repositorio.Plataforma.PossuiJogosAsync(id))
        {
            throw new BusinessRuleException(BusinessErrors.PLATAFORMA_POSSUI_JOGOS); // RN02
        }

        await repositorio.Plataforma.ExcluirAsync(id);
    }
}
