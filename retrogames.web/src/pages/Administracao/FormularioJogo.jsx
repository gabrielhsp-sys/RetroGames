import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { condicaoJogoApi, generoApi, jogoApi, plataformaApi } from '../../api/servicos'
import { ehNaoEncontrado, obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { Carregando, MensagemErro } from '../../components/MensagensEstado'

const FORMULARIO_VAZIO = {
  nome: '',
  descricao: '',
  anoLancamento: '',
  preco: '',
  estoque: '',
  idPlataforma: '',
  idCondicaoJogo: '',
  idsGeneros: [],
  ativo: true,
}

// /admin/jogos/novo inclui; /admin/jogos/:id/editar carrega o jogo e altera.
export default function FormularioJogo() {
  const { id } = useParams()
  const ehEdicao = id !== undefined
  const navigate = useNavigate()

  const [plataformas, setPlataformas] = useState([])
  const [condicoes, setCondicoes] = useState([])
  const [generos, setGeneros] = useState([])
  const [formulario, setFormulario] = useState(FORMULARIO_VAZIO)
  const [nomeOriginal, setNomeOriginal] = useState('')
  const [carregando, setCarregando] = useState(true)
  const [erroCarga, setErroCarga] = useState('')
  const [jogoNaoEncontrado, setJogoNaoEncontrado] = useState(false)
  const [errosCampos, setErrosCampos] = useState({})
  const [erroFormulario, setErroFormulario] = useState('')
  const [salvando, setSalvando] = useState(false)

  const carregarDados = useCallback(() => {
    setCarregando(true)
    setErroCarga('')
    setJogoNaoEncontrado(false)
    Promise.all([
      plataformaApi.listar(),
      condicaoJogoApi.listar(),
      generoApi.listar(),
      ehEdicao ? jogoApi.buscarPorId(id) : Promise.resolve(null),
    ])
      .then(([listaPlataformas, listaCondicoes, listaGeneros, jogo]) => {
        setPlataformas(listaPlataformas)
        setCondicoes(listaCondicoes)
        setGeneros(listaGeneros)
        if (jogo) {
          // Na edição, os dados atuais, com os gêneros já marcados.
          setNomeOriginal(jogo.nome)
          setFormulario({
            nome: jogo.nome,
            descricao: jogo.descricao ?? '',
            anoLancamento: String(jogo.anoLancamento),
            preco: String(jogo.preco),
            estoque: String(jogo.estoque),
            idPlataforma: String(jogo.idPlataforma),
            idCondicaoJogo: String(jogo.idCondicaoJogo),
            idsGeneros: jogo.generos.map((g) => g.id),
            ativo: jogo.ativo,
          })
        }
      })
      .catch((falha) => {
        setErroCarga(obterMensagemErro(falha))
        setJogoNaoEncontrado(ehNaoEncontrado(falha))
      })
      .finally(() => setCarregando(false))
  }, [ehEdicao, id])

  useEffect(() => {
    carregarDados()
  }, [carregarDados])

  function alterarCampo(evento) {
    const { name, value, type, checked } = evento.target
    setFormulario({ ...formulario, [name]: type === 'checkbox' ? checked : value })
  }

  function alternarGenero(idGenero) {
    const marcados = formulario.idsGeneros.includes(idGenero)
      ? formulario.idsGeneros.filter((g) => g !== idGenero)
      : [...formulario.idsGeneros, idGenero]
    setFormulario({ ...formulario, idsGeneros: marcados })
  }

  async function salvar(evento) {
    evento.preventDefault()
    setSalvando(true)
    setErrosCampos({})
    setErroFormulario('')
    // Number('') vale 0, e estoque 0 é válido: sem esta checagem, o campo em
    // branco seria gravado como zero sem aviso nenhum.
    if (String(formulario.estoque).trim() === '') {
      setErrosCampos({ estoque: 'Informe o estoque (0 se não houver unidades).' })
      setSalvando(false)
      return
    }
    // Os campos devolvem texto: os números são convertidos com Number() antes de enviar.
    const dados = {
      nome: formulario.nome.trim(),
      descricao: formulario.descricao.trim() || null,
      anoLancamento: Number(formulario.anoLancamento),
      preco: Number(formulario.preco),
      estoque: Number(formulario.estoque),
      idPlataforma: Number(formulario.idPlataforma),
      idCondicaoJogo: Number(formulario.idCondicaoJogo),
      idsGeneros: formulario.idsGeneros,
    }
    try {
      if (ehEdicao) {
        await jogoApi.alterar(id, { ...dados, ativo: formulario.ativo })
        navigate('/admin/jogos', { state: { aviso: `"${dados.nome}" alterado.` } })
      } else {
        await jogoApi.inserir(dados)
        navigate('/admin/jogos', { state: { aviso: `"${dados.nome}" incluído.` } })
      }
    } catch (falha) {
      // 400 com código 3: erro no campo certo. 404 (100, 104, 105): a mensagem.
      setErrosCampos(obterErrosDosCampos(falha))
      setErroFormulario(obterMensagemErro(falha))
      setSalvando(false)
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  }

  const classe = (base, campo) => `${base} ${errosCampos[campo] ? 'is-invalid' : ''}`
  const descricaoErro = (campo, ajuda) =>
    [errosCampos[campo] ? `erro-${campo}` : null, ajuda].filter(Boolean).join(' ') || undefined
  const mensagemDoCampo = (campo) => (
    <div id={`erro-${campo}`} className="invalid-feedback">{errosCampos[campo]}</div>
  )

  const titulo = ehEdicao ? `Alterar jogo${nomeOriginal ? `: ${nomeOriginal}` : ''}` : 'Novo jogo'

  return (
    <section>
      <Link to="/admin/jogos" className="d-inline-flex align-items-center mb-3 fw-semibold link-voltar">
        <span aria-hidden="true">←</span>&nbsp;Voltar à lista de jogos
      </Link>
      <h1 className="titulo-pagina mb-4">{titulo}</h1>

      {carregando && <Carregando texto="Carregando formulário..." />}
      <MensagemErro mensagem={erroCarga} aoTentarNovamente={jogoNaoEncontrado ? undefined : carregarDados} />

      {!carregando && !erroCarga && (
        <form className="painel p-4" onSubmit={salvar} noValidate>
          <MensagemErro mensagem={erroFormulario} />

          <div className="row g-3">
            <div className="col-12 col-md-8">
              <label htmlFor="nome" className="form-label">Nome</label>
              <input
                id="nome" name="nome" className={classe('form-control', 'nome')}
                value={formulario.nome} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.nome)} aria-describedby={descricaoErro('nome')}
              />
              {mensagemDoCampo('nome')}
            </div>
            <div className="col-12 col-md-4">
              <label htmlFor="anoLancamento" className="form-label">Ano de lançamento</label>
              <input
                id="anoLancamento" name="anoLancamento" type="number" inputMode="numeric"
                min="1970" max="2100" className={classe('form-control', 'anoLancamento')}
                value={formulario.anoLancamento} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.anoLancamento)}
                aria-describedby={descricaoErro('anoLancamento', 'ajuda-ano')}
              />
              <div id="ajuda-ano" className="form-text">Entre 1970 e 2100.</div>
              {mensagemDoCampo('anoLancamento')}
            </div>

            <div className="col-12">
              <label htmlFor="descricao" className="form-label">
                Descrição <span className="fw-normal">(opcional)</span>
              </label>
              <textarea
                id="descricao" name="descricao" rows="3" className={classe('form-control', 'descricao')}
                value={formulario.descricao} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.descricao)} aria-describedby={descricaoErro('descricao')}
              />
              {mensagemDoCampo('descricao')}
            </div>

            <div className="col-6 col-md-3">
              <label htmlFor="preco" className="form-label">Preço (R$)</label>
              <input
                id="preco" name="preco" type="number" inputMode="decimal" step="0.01" min="0.01"
                className={classe('form-control', 'preco')}
                value={formulario.preco} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.preco)} aria-describedby={descricaoErro('preco', 'ajuda-preco')}
              />
              <div id="ajuda-preco" className="form-text">Maior que zero.</div>
              {mensagemDoCampo('preco')}
            </div>
            <div className="col-6 col-md-3">
              <label htmlFor="estoque" className="form-label">Estoque</label>
              <input
                id="estoque" name="estoque" type="number" inputMode="numeric" min="0"
                className={classe('form-control', 'estoque')}
                value={formulario.estoque} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.estoque)} aria-describedby={descricaoErro('estoque')}
              />
              {mensagemDoCampo('estoque')}
            </div>
            <div className="col-12 col-md-3">
              <label htmlFor="idPlataforma" className="form-label">Plataforma</label>
              <select
                id="idPlataforma" name="idPlataforma" className={classe('form-select', 'idPlataforma')}
                value={formulario.idPlataforma} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.idPlataforma)} aria-describedby={descricaoErro('idPlataforma')}
              >
                <option value="">Escolha...</option>
                {plataformas.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
              </select>
              {mensagemDoCampo('idPlataforma')}
            </div>
            <div className="col-12 col-md-3">
              <label htmlFor="idCondicaoJogo" className="form-label">Condição</label>
              <select
                id="idCondicaoJogo" name="idCondicaoJogo" className={classe('form-select', 'idCondicaoJogo')}
                value={formulario.idCondicaoJogo} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.idCondicaoJogo)} aria-describedby={descricaoErro('idCondicaoJogo')}
              >
                <option value="">Escolha...</option>
                {condicoes.map((c) => <option key={c.id} value={c.id}>{c.descricao}</option>)}
              </select>
              {mensagemDoCampo('idCondicaoJogo')}
            </div>

            <div className="col-12">
              <fieldset aria-describedby={descricaoErro('idsGeneros', 'ajuda-generos')}>
                <legend className="form-label fs-6">Gêneros</legend>
                <div id="ajuda-generos" className="form-text mt-0 mb-1">Marque ao menos um.</div>
                <div className={`grade-generos ${errosCampos.idsGeneros ? 'is-invalid' : ''}`}>
                  {generos.map((g) => (
                    <div className="form-check" key={g.id}>
                      <input
                        id={`genero-${g.id}`} type="checkbox" className="form-check-input"
                        checked={formulario.idsGeneros.includes(g.id)}
                        onChange={() => alternarGenero(g.id)}
                      />
                      <label htmlFor={`genero-${g.id}`} className="form-check-label">{g.nome}</label>
                    </div>
                  ))}
                </div>
                {mensagemDoCampo('idsGeneros')}
              </fieldset>
            </div>

            {ehEdicao && (
              <div className="col-12">
                <div className="form-check form-switch">
                  <input
                    id="ativo" name="ativo" type="checkbox" role="switch" className="form-check-input"
                    checked={formulario.ativo} onChange={alterarCampo}
                  />
                  <label htmlFor="ativo" className="form-check-label">
                    Ativo (aparece no catálogo da loja)
                  </label>
                </div>
              </div>
            )}
          </div>

          <div className="d-flex flex-wrap gap-2 mt-4">
            <button type="submit" className="btn btn-primary" disabled={salvando}>
              {salvando ? 'Salvando...' : ehEdicao ? 'Salvar alteração' : 'Incluir jogo'}
            </button>
            <Link to="/admin/jogos" className="btn btn-outline-primary">Cancelar</Link>
          </div>
        </form>
      )}
    </section>
  )
}
