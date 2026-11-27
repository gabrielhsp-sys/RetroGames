using RetroGames.Model.Model;
using RetroGames.Repository;

namespace RetroGames.Services;

/// <summary>
/// Regras de negócio de gêneros (C08), no mesmo desenho do PlataformaService.
/// Não conhece SQL nem HTTP: usa só o IRepositorySet e lança
/// BusinessRuleException com um erro do catálogo.
/// </summary>
public class GeneroService(IRepositorySet repositorio) : IGeneroService
{
    public async Task<List<Genero>> ListarAsync()
    {
        return await repositorio.Genero.ListarAsync();
    }

    public async Task<Genero> BuscarPorIdAsync(int id)
    {
        var genero = await repositorio.Genero.BuscarPorIdAsync(id);
        if (genero == null)
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NAO_ENCONTRADO);
        }
        return genero;
    }

    public async Task<Genero> InserirAsync(Genero genero)
    {
        genero.Nome = genero.Nome.Trim();
        if (await repositorio.Genero.ExisteComNomeAsync(genero.Nome))
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NOME_DUPLICADO);
        }

        try
        {
            genero.Id = await repositorio.Genero.InserirAsync(genero);
        }
        catch (RegistroDuplicadoException) // outra gravação com o mesmo nome chegou antes
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NOME_DUPLICADO);
        }
        return genero;
    }

    public async Task AlterarAsync(Genero genero)
    {
        if (await repositorio.Genero.BuscarPorIdAsync(genero.Id) == null)
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NAO_ENCONTRADO);
        }

        genero.Nome = genero.Nome.Trim();
        // Ignora o próprio gênero: manter o próprio nome não é duplicidade.
        if (await repositorio.Genero.ExisteComNomeAsync(genero.Nome, genero.Id))
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NOME_DUPLICADO);
        }

        try
        {
            await repositorio.Genero.AlterarAsync(genero);
        }
        catch (RegistroDuplicadoException) // outra gravação com o mesmo nome chegou antes
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NOME_DUPLICADO);
        }
    }

    public async Task ExcluirAsync(int id)
    {
        if (await repositorio.Genero.BuscarPorIdAsync(id) == null)
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_NAO_ENCONTRADO);
        }

        // Os jogos do gênero estão na tabela de vínculo, lida pelo seu próprio Repository.
        if (await repositorio.JogoGenero.ExisteComGeneroAsync(id))
        {
            throw new BusinessRuleException(BusinessErrors.GENERO_POSSUI_JOGOS);
        }

        await repositorio.Genero.ExcluirAsync(id);
    }
}
