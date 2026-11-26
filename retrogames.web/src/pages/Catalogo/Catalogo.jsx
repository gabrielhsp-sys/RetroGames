import { useCallback, useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { generoApi, jogoApi, plataformaApi } from '../../api/servicos'
import { obterMensagemErro } from '../../api/erros'
import CardJogo from '../../components/CardJogo'
import Paginacao from '../../components/Paginacao'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'

const TAMANHO_PAGINA = 12

export default function Catalogo() {
  // Os filtros vivem na URL (?busca=mario&idPlataforma=2&pagina=2): o link pode ser
  // compartilhado, o F5 mantém a busca e o botão Voltar do navegador funciona.
  const [parametros, setParametros] = useSearchParams()
  const busca = parametros.get('busca') ?? ''
  const idPlataforma = parametros.get('idPlataforma') ?? ''
  const idGenero = parametros.get('idGenero') ?? ''
  const pagina = Number(parametros.get('pagina')) || 1 // ?pagina=abc vira 1

  const [plataformas, setPlataformas] = useState([])
  const [generos, setGeneros] = useState([])
  const [resultado, setResultado] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [erroListas, setErroListas] = useState('')
  // Listas dos filtros: carregam uma vez (e de novo no Tentar novamente, se falharam).
  const carregarListas = useCallback(() => {
    setErroListas('')
    Promise.all([plataformaApi.listar(), generoApi.listar()])
      .then(([listaPlataformas, listaGeneros]) => {
        setPlataformas(listaPlataformas)
        setGeneros(listaGeneros)
      })
      .catch((falha) => setErroListas(obterMensagemErro(falha)))
  }, [])

  useEffect(() => {
    carregarListas()
  }, [carregarListas])

  // Jogos: recarregam sempre que um filtro da URL muda.
  const carregarJogos = useCallback(() => {
    setCarregando(true)
    setErro('')
    jogoApi
      .listar({
        busca: busca || undefined,
        idPlataforma: idPlataforma || undefined,
        idGenero: idGenero || undefined,
        pagina,
        tamanhoPagina: TAMANHO_PAGINA,
      })
      .then(setResultado)
      .catch((falha) => setErro(obterMensagemErro(falha)))
      .finally(() => setCarregando(false))
  }, [busca, idPlataforma, idGenero, pagina])

  useEffect(() => {
    carregarJogos()
  }, [carregarJogos])

  function alterarFiltro(nome, valor) {
    const novos = new URLSearchParams(parametros)
    if (valor) novos.set(nome, valor)
    else novos.delete(nome)
    if (nome !== 'pagina') novos.delete('pagina') // trocou o filtro: volta à página 1
    setParametros(novos)
  }

  const temFiltro = Boolean(busca || idPlataforma || idGenero)

  function enviarBusca(evento) {
    evento.preventDefault()
    // O texto digitado só vira filtro ao enviar a busca (Enter ou botão).
    const texto = new FormData(evento.currentTarget).get('busca')
    alterarFiltro('busca', String(texto).trim())
  }

  return (
    <section>
      <h1 className="titulo-pagina">Catálogo</h1>
      <p className="subtitulo-pagina">Cartuchos e jogos retrô usados, conferidos e com preço na etiqueta.</p>

      <form className="painel row g-2 align-items-end mx-0 mb-4" onSubmit={enviarBusca} role="search">
        <div className="col-12 col-md-6">
          <label htmlFor="busca" className="form-label">Buscar</label>
          <div className="input-group">
            <input
              id="busca"
              name="busca"
              type="search"
              className="form-control"
              key={busca}
              defaultValue={busca}
            />
            <button type="submit" className="btn btn-primary">Buscar</button>
          </div>
        </div>
        <div className="col-6 col-md-3">
          <label htmlFor="idPlataforma" className="form-label">Plataforma</label>
          <select
            id="idPlataforma"
            className="form-select"
            value={idPlataforma}
            onChange={(e) => alterarFiltro('idPlataforma', e.target.value)}
          >
            <option value="">Todas</option>
            {plataformas.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </div>
        <div className="col-6 col-md-3">
          <label htmlFor="idGenero" className="form-label">Gênero</label>
          <select
            id="idGenero"
            className="form-select"
            value={idGenero}
            onChange={(e) => alterarFiltro('idGenero', e.target.value)}
          >
            <option value="">Todos</option>
            {generos.map((g) => <option key={g.id} value={g.id}>{g.nome}</option>)}
          </select>
        </div>
        {temFiltro && (
          <div className="col-12">
            <button type="button" className="btn btn-link px-0" onClick={() => setParametros({})}>
              Limpar busca e filtros
            </button>
          </div>
        )}
      </form>

      {carregando && <Carregando texto="Carregando jogos..." />}
      <MensagemErro mensagem={erro} aoTentarNovamente={carregarJogos} />
      {/* Só avisa das listas quando os jogos carregaram: evita dois alertas iguais. */}
      {!erro && (
        <MensagemErro
          mensagem={erroListas && `Os filtros não carregaram. ${erroListas}`}
          aoTentarNovamente={carregarListas}
        />
      )}

      {!carregando && !erro && resultado?.itens.length === 0 && (
        <EstadoVazio titulo="Nenhum jogo encontrado.">
          <p>Tente outro nome ou outra plataforma.</p>
          {temFiltro && (
            <button type="button" className="btn btn-outline-primary" onClick={() => setParametros({})}>
              Ver o catálogo inteiro
            </button>
          )}
        </EstadoVazio>
      )}

      {!carregando && !erro && resultado?.itens.length > 0 && (
        <>
          <p className="mb-4" aria-live="polite">
            {resultado.totalItens} {resultado.totalItens === 1 ? 'jogo encontrado' : 'jogos encontrados'}
            {resultado.totalPaginas > 1 && ` · página ${resultado.pagina} de ${resultado.totalPaginas}`}
          </p>
          <div className="row row-cols-1 row-cols-md-3 row-cols-lg-4 prateleira">
            {resultado.itens.map((jogo) => (
              <div className="col" key={jogo.id}>
                <CardJogo jogo={jogo} />
              </div>
            ))}
          </div>
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
