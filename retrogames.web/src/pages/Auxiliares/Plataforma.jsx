import { useCallback, useEffect, useRef, useState } from 'react'
import { plataformaApi } from '../../api/servicos'
import { obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'

const FORMULARIO_VAZIO = { nome: '', fabricante: '', anoLancamento: '' }

export default function Plataforma() {
  const [plataformas, setPlataformas] = useState([])
  const [carregando, setCarregando] = useState(true)
  const [erroLista, setErroLista] = useState('')

  // Formulário: sem plataforma em edição, inclui; com, altera.
  const [formulario, setFormulario] = useState(FORMULARIO_VAZIO)
  const [emEdicao, setEmEdicao] = useState(null)
  const [errosCampos, setErrosCampos] = useState({})
  const [erroFormulario, setErroFormulario] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [aviso, setAviso] = useState('')
  const campoNome = useRef(null)

  const carregarPlataformas = useCallback(() => {
    setCarregando(true)
    setErroLista('')
    plataformaApi
      .listar()
      .then(setPlataformas)
      .catch((falha) => setErroLista(obterMensagemErro(falha)))
      .finally(() => setCarregando(false))
  }, [])

  useEffect(() => {
    carregarPlataformas()
  }, [carregarPlataformas])

  function alterarCampo(evento) {
    setFormulario({ ...formulario, [evento.target.name]: evento.target.value })
  }

  function limparFormulario() {
    setFormulario(FORMULARIO_VAZIO)
    setEmEdicao(null)
    setErrosCampos({})
    setErroFormulario('')
  }

  function editar(plataforma) {
    setEmEdicao(plataforma)
    setFormulario({
      nome: plataforma.nome,
      fabricante: plataforma.fabricante,
      anoLancamento: String(plataforma.anoLancamento),
    })
    setErrosCampos({})
    setErroFormulario('')
    setAviso('')
    campoNome.current?.focus()
  }

  async function salvar(evento) {
    evento.preventDefault()
    setSalvando(true)
    setErrosCampos({})
    setErroFormulario('')
    setAviso('')
    // Os campos do formulário devolvem texto: o ano vai para a API como número.
    const dados = { ...formulario, anoLancamento: Number(formulario.anoLancamento) }
    try {
      if (emEdicao) {
        await plataformaApi.alterar(emEdicao.id, dados)
        setAviso(`Plataforma "${dados.nome}" alterada.`)
      } else {
        await plataformaApi.inserir(dados)
        setAviso(`Plataforma "${dados.nome}" incluída.`)
      }
      limparFormulario()
      carregarPlataformas() // a lista é recarregada, sem recarregar a página
    } catch (falha) {
      // 400 com código 3: erro embaixo de cada campo. 409 (101): só a mensagem.
      setErrosCampos(obterErrosDosCampos(falha))
      setErroFormulario(obterMensagemErro(falha))
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(plataforma) {
    if (!window.confirm(`Excluir a plataforma "${plataforma.nome}"? Esta ação não pode ser desfeita.`)) return
    setAviso('')
    setErroLista('')
    try {
      await plataformaApi.excluir(plataforma.id)
      if (emEdicao?.id === plataforma.id) limparFormulario()
      setAviso(`Plataforma "${plataforma.nome}" excluída.`)
      carregarPlataformas()
    } catch (falha) {
      // 409, código 102: a plataforma tem jogos e continua na lista.
      setErroLista(obterMensagemErro(falha))
    }
  }

  // Classe e descrição de erro de um campo (is-invalid do Bootstrap).
  const classeCampo = (campo) => `form-control ${errosCampos[campo] ? 'is-invalid' : ''}`
  const descricaoErro = (campo) => (errosCampos[campo] ? `erro-${campo}` : undefined)

  return (
    <section>
      <h1 className="titulo-pagina">Plataformas</h1>
      <p className="subtitulo-pagina">Consoles e computadores em que os jogos da loja rodam.</p>

      {aviso && (
        <div role="status" className="estado estado-sucesso">
          <span className="estado-icone" aria-hidden="true">✓</span>
          <span className="fw-semibold">{aviso}</span>
        </div>
      )}

      <div className="row g-4">
        <div className="col-12 col-lg-4">
          <form className="painel p-4" onSubmit={salvar} noValidate>
            <h2 className="h5 mb-3">
              {emEdicao ? `Alterar: ${emEdicao.nome}` : 'Nova plataforma'}
            </h2>

            <MensagemErro mensagem={erroFormulario} />

            <div className="mb-3">
              <label htmlFor="nome" className="form-label">Nome</label>
              <input
                ref={campoNome}
                id="nome"
                name="nome"
                className={classeCampo('nome')}
                value={formulario.nome}
                onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.nome)}
                aria-describedby={descricaoErro('nome')}
              />
              <div id="erro-nome" className="invalid-feedback">{errosCampos.nome}</div>
            </div>
            <div className="mb-3">
              <label htmlFor="fabricante" className="form-label">Fabricante</label>
              <input
                id="fabricante"
                name="fabricante"
                className={classeCampo('fabricante')}
                value={formulario.fabricante}
                onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.fabricante)}
                aria-describedby={descricaoErro('fabricante')}
              />
              <div id="erro-fabricante" className="invalid-feedback">{errosCampos.fabricante}</div>
            </div>
            <div className="mb-4">
              <label htmlFor="anoLancamento" className="form-label">Ano de lançamento</label>
              <input
                id="anoLancamento"
                name="anoLancamento"
                type="number"
                inputMode="numeric"
                className={classeCampo('anoLancamento')}
                value={formulario.anoLancamento}
                onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.anoLancamento)}
                aria-describedby={descricaoErro('anoLancamento')}
              />
              <div id="erro-anoLancamento" className="invalid-feedback">{errosCampos.anoLancamento}</div>
            </div>

            <div className="d-flex flex-wrap gap-2">
              <button type="submit" className="btn btn-primary" disabled={salvando}>
                {salvando ? 'Salvando...' : emEdicao ? 'Salvar alteração' : 'Incluir plataforma'}
              </button>
              {emEdicao && (
                <button type="button" className="btn btn-outline-primary" onClick={limparFormulario}>
                  Cancelar
                </button>
              )}
            </div>
          </form>
        </div>

        <div className="col-12 col-lg-8">
          {carregando && <Carregando texto="Carregando plataformas..." />}
          <MensagemErro
            mensagem={erroLista}
            aoTentarNovamente={plataformas.length === 0 ? carregarPlataformas : undefined}
          />

          {!carregando && plataformas.length === 0 && !erroLista && (
            <EstadoVazio titulo="Nenhuma plataforma cadastrada.">
              <p>Use o formulário para incluir a primeira.</p>
            </EstadoVazio>
          )}

          {!carregando && plataformas.length > 0 && (
            <div className="painel p-0 table-responsive">
              <table className="table tabela-admin align-middle mb-0">
                <caption className="visually-hidden">Plataformas cadastradas</caption>
                <thead>
                  <tr>
                    <th scope="col">Nome</th>
                    <th scope="col">Fabricante</th>
                    <th scope="col">Ano</th>
                    <th scope="col" className="text-end">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {plataformas.map((plataforma) => (
                    <tr key={plataforma.id} className={emEdicao?.id === plataforma.id ? 'linha-em-edicao' : ''}>
                      <td className="fw-semibold">{plataforma.nome}</td>
                      <td>{plataforma.fabricante}</td>
                      <td>{plataforma.anoLancamento}</td>
                      <td className="text-end text-nowrap">
                        <button
                          type="button"
                          className="btn btn-outline-primary btn-sm me-2"
                          onClick={() => editar(plataforma)}
                          aria-label={`Editar ${plataforma.nome}`}
                        >
                          Editar
                        </button>
                        <button
                          type="button"
                          className="btn btn-outline-danger btn-sm"
                          onClick={() => excluir(plataforma)}
                          aria-label={`Excluir ${plataforma.nome}`}
                        >
                          Excluir
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </section>
  )
}
