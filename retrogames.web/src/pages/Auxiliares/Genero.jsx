import { useCallback, useEffect, useRef, useState } from 'react'
import { generoApi } from '../../api/servicos'
import { obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'

// C08: manutenção de gêneros, no mesmo desenho da tela de Plataformas.
const FORMULARIO_VAZIO = { nome: '' }
const TAMANHO_MAXIMO_NOME = 50 // no_genero é VARCHAR(50)

export default function Genero() {
  const [generos, setGeneros] = useState([])
  const [carregando, setCarregando] = useState(true)
  const [erroLista, setErroLista] = useState('')

  // Formulário: sem gênero em edição, inclui; com, altera.
  const [formulario, setFormulario] = useState(FORMULARIO_VAZIO)
  const [emEdicao, setEmEdicao] = useState(null)
  const [errosCampos, setErrosCampos] = useState({})
  const [erroFormulario, setErroFormulario] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [aviso, setAviso] = useState('')
  const campoNome = useRef(null)
  // Áreas que recebem o foco depois de excluir: no celular a mensagem fica
  // longe do botão clicado, e a linha excluída some com o foco.
  const areaAviso = useRef(null)
  const areaErroLista = useRef(null)

  const carregarGeneros = useCallback(() => {
    setCarregando(true)
    setErroLista('')
    generoApi
      .listar()
      .then(setGeneros)
      .catch((falha) => setErroLista(obterMensagemErro(falha)))
      .finally(() => setCarregando(false))
  }, [])

  useEffect(() => {
    carregarGeneros()
  }, [carregarGeneros])

  function alterarCampo(evento) {
    setFormulario({ ...formulario, [evento.target.name]: evento.target.value })
  }

  function limparFormulario() {
    setFormulario(FORMULARIO_VAZIO)
    setEmEdicao(null)
    setErrosCampos({})
    setErroFormulario('')
  }

  function editar(genero) {
    setEmEdicao(genero)
    setFormulario({ nome: genero.nome })
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
    const dados = { nome: formulario.nome }
    try {
      if (emEdicao) {
        await generoApi.alterar(emEdicao.id, dados)
        setAviso(`Gênero "${dados.nome.trim()}" alterado.`)
      } else {
        await generoApi.inserir(dados)
        setAviso(`Gênero "${dados.nome.trim()}" incluído.`)
      }
      limparFormulario()
      carregarGeneros() // a lista é recarregada, sem recarregar a página
    } catch (falha) {
      // 400 com código 3: erro embaixo do campo. 409 (107): só a mensagem.
      setErrosCampos(obterErrosDosCampos(falha))
      setErroFormulario(obterMensagemErro(falha))
    } finally {
      setSalvando(false)
    }
  }

  async function excluir(genero) {
    if (!window.confirm(`Excluir o gênero "${genero.nome}"? Esta ação não pode ser desfeita.`)) return
    setAviso('')
    setErroLista('')
    try {
      await generoApi.excluir(genero.id)
      if (emEdicao?.id === genero.id) limparFormulario()
      setAviso(`Gênero "${genero.nome}" excluído.`)
      focarDepois(areaAviso)
      carregarGeneros()
    } catch (falha) {
      // 409, código 108: o gênero tem jogos e continua na lista.
      setErroLista(obterMensagemErro(falha))
      focarDepois(areaErroLista)
    }
  }

  // Leva o foco (e a rolagem) até a mensagem depois que a tela atualizar.
  function focarDepois(area) {
    requestAnimationFrame(() => area.current?.focus())
  }

  // Classe e descrição de erro de um campo (is-invalid do Bootstrap).
  const classeCampo = (campo) => `form-control ${errosCampos[campo] ? 'is-invalid' : ''}`
  const descricaoNome = errosCampos.nome ? 'ajuda-nome erro-nome' : 'ajuda-nome'

  // Editar e Excluir: na coluna Ações no computador; embaixo do nome no celular.
  const acoes = (genero) => (
    <>
      <button
        type="button"
        className="btn btn-outline-primary btn-sm"
        onClick={() => editar(genero)}
        aria-label={`Editar ${genero.nome}`}
      >
        Editar
      </button>
      <button
        type="button"
        className="btn btn-outline-danger btn-sm"
        onClick={() => excluir(genero)}
        aria-label={`Excluir ${genero.nome}`}
      >
        Excluir
      </button>
    </>
  )

  return (
    <section>
      <h1 className="titulo-pagina">Gêneros</h1>
      <p className="subtitulo-pagina">Os tipos de jogo usados nos filtros do catálogo e no cadastro de jogos.</p>

      <div ref={areaAviso} tabIndex={-1}>
        {aviso && (
          <div role="status" className="estado estado-sucesso">
            <span className="estado-icone" aria-hidden="true">✓</span>
            <span className="fw-semibold text-break">{aviso}</span>
          </div>
        )}
      </div>

      <div className="row g-4">
        <div className="col-12 col-lg-4">
          <form className="painel p-4" onSubmit={salvar} noValidate>
            <h2 className="h5 mb-3 text-break">
              {emEdicao ? `Alterar: ${emEdicao.nome}` : 'Novo gênero'}
            </h2>

            <MensagemErro mensagem={erroFormulario} />

            <div className="mb-4">
              <label htmlFor="nome" className="form-label">Nome</label>
              <input
                ref={campoNome}
                id="nome"
                name="nome"
                className={classeCampo('nome')}
                value={formulario.nome}
                onChange={alterarCampo}
                maxLength={TAMANHO_MAXIMO_NOME}
                aria-invalid={Boolean(errosCampos.nome)}
                aria-describedby={descricaoNome}
              />
              <div id="erro-nome" className="invalid-feedback">{errosCampos.nome}</div>
              <div id="ajuda-nome" className="form-text">
                Até {TAMANHO_MAXIMO_NOME} caracteres. Cada gênero tem um nome único.
              </div>
            </div>

            <div className="d-flex flex-wrap gap-2">
              <button type="submit" className="btn btn-primary" disabled={salvando}>
                {salvando ? 'Salvando...' : emEdicao ? 'Salvar alteração' : 'Incluir gênero'}
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
          {carregando && <Carregando texto="Carregando gêneros..." />}
          <div ref={areaErroLista} tabIndex={-1}>
            <MensagemErro
              mensagem={erroLista}
              aoTentarNovamente={generos.length === 0 ? carregarGeneros : undefined}
            />
          </div>

          {!carregando && generos.length === 0 && !erroLista && (
            <EstadoVazio titulo="Nenhum gênero cadastrado.">
              <p>Use o formulário para incluir o primeiro.</p>
            </EstadoVazio>
          )}

          {!carregando && generos.length > 0 && (
            <div className="painel p-0 table-responsive">
              <table className="table tabela-admin align-middle mb-0">
                <caption className="visually-hidden">Gêneros cadastrados</caption>
                <thead>
                  <tr>
                    <th scope="col">Nome</th>
                    <th scope="col" className="text-end d-none d-md-table-cell">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {generos.map((genero) => (
                    <tr key={genero.id} className={emEdicao?.id === genero.id ? 'linha-em-edicao' : ''}>
                      <td>
                        <span className="fw-semibold text-break">{genero.nome}</span>
                        <div className="d-flex d-md-none flex-wrap gap-2 mt-2">{acoes(genero)}</div>
                      </td>
                      <td className="text-end text-nowrap d-none d-md-table-cell">
                        <div className="d-inline-flex gap-2">{acoes(genero)}</div>
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
