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
  const pagina = Number(parametros.get('pagina') ?? 1)

  const [plataformas, setPlataformas] = useState([])
  const [generos, setGeneros] = useState([])
  const [resultado, setResultado] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  // Listas dos filtros: carregam uma vez.
  useEffect(() => {
    plataformaApi.listar().then(setPlataformas).catch(() => setPlataformas([]))
    generoApi.listar().then(setGeneros).catch(() => setGeneros([]))
  }, [])

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

  function enviarBusca(evento) {
    evento.preventDefault()
    // O texto digitado só vira filtro ao enviar a busca (Enter ou botão).
    const texto = new FormData(evento.currentTarget).get('busca')
    alterarFiltro('busca', String(texto).trim())
  }

  return (
    <section>
      <h1 className="h3 mb-3">Catálogo</h1>

      <form className="row g-2 align-items-end mb-4" onSubmit={enviarBusca} role="search">
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
      </form>

      {carregando && <Carregando texto="Carregando jogos..." />}
      <MensagemErro mensagem={erro} aoTentarNovamente={carregarJogos} />

      {!carregando && !erro && resultado?.itens.length === 0 && (
        <EstadoVazio titulo="Nenhum jogo encontrado.">
          <p>Tente outro nome ou limpe os filtros.</p>
        </EstadoVazio>
      )}

      {!carregando && !erro && resultado?.itens.length > 0 && (
        <>
          <div className="row row-cols-1 row-cols-sm-2 row-cols-lg-4 g-3">
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
