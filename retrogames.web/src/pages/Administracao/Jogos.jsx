import { useCallback, useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { jogoApi } from '../../api/servicos'
import { obterMensagemErro } from '../../api/erros'
import { formatarPreco } from '../../lib/formato'
import Paginacao from '../../components/Paginacao'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'

const TAMANHO_PAGINA = 12

export default function Jogos() {
  const [parametros, setParametros] = useSearchParams()
  const busca = parametros.get('busca') ?? ''
  const pagina = Number(parametros.get('pagina')) || 1
  const local = useLocation()

  const [resultado, setResultado] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [erroAcao, setErroAcao] = useState('')
  // Aviso que o formulário deixa ao voltar para a lista ("Jogo incluído.").
  const [aviso, setAviso] = useState(local.state?.aviso ?? '')
  const navigate = useNavigate()

  // O aviso foi lido: apaga-o do histórico para não reaparecer no F5.
  useEffect(() => {
    if (local.state?.aviso) navigate(local.pathname + local.search, { replace: true, state: null })
  }, [local, navigate])

  const carregarJogos = useCallback(() => {
    setCarregando(true)
    setErro('')
    jogoApi
      // O administrador vê também os inativos (RN04).
      .listar({ busca: busca || undefined, pagina, tamanhoPagina: TAMANHO_PAGINA, incluirInativos: true })
      .then(setResultado)
      .catch((falha) => setErro(obterMensagemErro(falha)))
      .finally(() => setCarregando(false))
  }, [busca, pagina])

  useEffect(() => {
    carregarJogos()
  }, [carregarJogos])

  function alterarFiltro(nome, valor) {
    const novos = new URLSearchParams(parametros)
    if (valor) novos.set(nome, valor)
    else novos.delete(nome)
    if (nome !== 'pagina') novos.delete('pagina')
    setParametros(novos)
  }

  function enviarBusca(evento) {
    evento.preventDefault()
    alterarFiltro('busca', String(new FormData(evento.currentTarget).get('busca')).trim())
  }

  async function desativar(jogo) {
    if (!window.confirm(`Desativar "${jogo.nome}"? Ele sai do catálogo, mas continua nesta lista e pode ser reativado na edição.`)) return
    setAviso('')
    setErroAcao('')
    try {
      await jogoApi.desativar(jogo.id) // exclusão lógica: st_ativo = 0 (RN03)
      setAviso(`"${jogo.nome}" desativado. Ele não aparece mais no catálogo.`)
      carregarJogos()
    } catch (falha) {
      setErroAcao(obterMensagemErro(falha))
    }
  }

  // Editar e Desativar: na coluna Ações no computador; embaixo do nome no celular,
  // para não ficarem escondidos à direita da tabela.
  const acoes = (jogo) => (
    <>
      <Link
        to={`/admin/jogos/${jogo.id}/editar`}
        className="btn btn-outline-primary btn-sm"
        aria-label={`Editar ${jogo.nome}`}
      >
        Editar
      </Link>
      {jogo.ativo && (
        <button
          type="button"
          className="btn btn-outline-danger btn-sm"
          onClick={() => desativar(jogo)}
          aria-label={`Desativar ${jogo.nome}`}
        >
          Desativar
        </button>
      )}
    </>
  )

  return (
    <section>
      <div className="d-flex flex-wrap align-items-end justify-content-between gap-3 mb-3">
        <div>
          <h1 className="titulo-pagina">Jogos</h1>
          <p className="subtitulo-pagina mb-0">Todos os jogos da loja, inclusive os inativos.</p>
        </div>
        <Link to="/admin/jogos/novo" className="btn btn-primary">Novo jogo</Link>
      </div>

      {aviso && (
        <div role="status" className="estado estado-sucesso">
          <span className="estado-icone" aria-hidden="true">✓</span>
          <span className="fw-semibold">{aviso}</span>
        </div>
      )}
      <MensagemErro mensagem={erroAcao} />

      <form className="painel mb-4" onSubmit={enviarBusca} role="search">
        <label htmlFor="busca" className="form-label">Buscar por nome</label>
        <div className="input-group">
          <input id="busca" name="busca" type="search" className="form-control" key={busca} defaultValue={busca} />
          <button type="submit" className="btn btn-primary">Buscar</button>
        </div>
      </form>

      {carregando && <Carregando texto="Carregando jogos..." />}
      <MensagemErro mensagem={erro} aoTentarNovamente={carregarJogos} />

      {!carregando && !erro && resultado?.itens.length === 0 && (
        <EstadoVazio titulo="Nenhum jogo encontrado.">
          <p>{busca ? 'Tente outro nome.' : 'Cadastre o primeiro jogo da loja.'}</p>
          <Link to="/admin/jogos/novo" className="btn btn-outline-primary">Novo jogo</Link>
        </EstadoVazio>
      )}

      {!carregando && !erro && resultado?.itens.length > 0 && (
        <>
          <div className="painel p-0 table-responsive">
            <table className="table tabela-admin align-middle mb-0">
              <caption className="visually-hidden">Jogos cadastrados</caption>
              <thead>
                <tr>
                  <th scope="col">Nome</th>
                  <th scope="col" className="d-none d-sm-table-cell">Plataforma</th>
                  <th scope="col" className="d-none d-lg-table-cell">Condição</th>
                  <th scope="col" className="text-end">Preço</th>
                  <th scope="col" className="text-end d-none d-sm-table-cell">Estoque</th>
                  <th scope="col">Situação</th>
                  <th scope="col" className="text-end d-none d-md-table-cell">Ações</th>
                </tr>
              </thead>
              <tbody>
                {resultado.itens.map((jogo) => (
                  <tr key={jogo.id}>
                    <td>
                      <span className="fw-semibold">{jogo.nome}</span>
                      <span className="d-block d-sm-none small">{jogo.nomePlataforma}</span>
                      <div className="d-flex d-md-none flex-wrap gap-2 mt-2">{acoes(jogo)}</div>
                    </td>
                    <td className="d-none d-sm-table-cell">{jogo.nomePlataforma}</td>
                    <td className="d-none d-lg-table-cell">{jogo.descricaoCondicao}</td>
                    <td className="text-end text-nowrap">{formatarPreco(jogo.preco)}</td>
                    <td className="text-end d-none d-sm-table-cell">{jogo.estoque}</td>
                    <td>
                      <span className={`situacao ${jogo.ativo ? 'situacao-ativo' : 'situacao-inativo'}`}>
                        {jogo.ativo ? 'Ativo' : 'Inativo'}
                      </span>
                    </td>
                    <td className="text-end text-nowrap d-none d-md-table-cell">
                      <div className="d-inline-flex gap-2">{acoes(jogo)}</div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <p className="small mt-2 mb-0">
            {resultado.totalItens} {resultado.totalItens === 1 ? 'jogo' : 'jogos'}
          </p>
          <Paginacao
            pagina={resultado.pagina}
            totalPaginas={resultado.totalPaginas}
            aoMudar={(nova) => alterarFiltro('pagina', String(nova))}
          />
        </>
      )}
    </section>
  )
}
