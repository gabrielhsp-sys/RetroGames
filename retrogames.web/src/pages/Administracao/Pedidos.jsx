import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { pedidoApi, situacaoPedidoApi } from '../../api/servicos'
import { ehNaoEncontrado, obterMensagemErro } from '../../api/erros'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import { SeloSituacao } from '../../components/Pedido'
import { formatarDataHora, formatarPreco } from '../../lib/formato'

const ID_CANCELADO = 4 // tb_situacao_pedido: a única troca que pede confirmação

// C07: gestão de pedidos pelo administrador (/admin/pedidos?idSituacao=).
// O filtro fica na URL: o link pode ser guardado, e o Voltar do navegador o restaura.
export default function Pedidos() {
  const [parametros, setParametros] = useSearchParams()
  const filtro = parametros.get('idSituacao') ?? ''
  const filtroValido = filtro === '' || /^\d+$/.test(filtro)

  const [situacoes, setSituacoes] = useState([])
  const [erroSituacoes, setErroSituacoes] = useState('')
  const [tentativaSituacoes, setTentativaSituacoes] = useState(0)

  // A lista carregada guarda a chave (filtro + tentativa) a que pertence: enquanto a
  // chave atual for outra, a tela está carregando. Assim o efeito só chama a API.
  const [tentativa, setTentativa] = useState(0) // muda a cada "Tentar novamente"
  const chave = `${filtro}#${tentativa}`
  const [resultado, setResultado] = useState({ chave: null, pedidos: [], erro: '', semSaida: '' })

  useEffect(() => {
    let ativo = true // resposta que chega depois de sair da tela é ignorada
    situacaoPedidoApi
      .listar()
      .then((lista) => ativo && setSituacoes(lista))
      .catch((falha) => ativo && setErroSituacoes(obterMensagemErro(falha)))
    return () => {
      ativo = false
    }
  }, [tentativaSituacoes])

  useEffect(() => {
    if (!filtroValido) return undefined
    const resposta = { vale: true } // resposta que chega depois de trocar o filtro ou sair é ignorada
    pedidoApi
      .listar(filtro || undefined)
      .then((pedidos) => resposta.vale && setResultado({ chave, pedidos, erro: '', semSaida: '' }))
      .catch((falha) => resposta.vale && setResultado(resultadoDaFalha(chave, falha)))
    return () => {
      resposta.vale = false
    }
  }, [chave, filtro, filtroValido])

  const carregando = filtroValido && resultado.chave !== chave
  const pedidos = resultado.pedidos
  const erro = carregando ? '' : resultado.erro
  const semSaida = filtroValido ? resultado.semSaida : 'Situação de pedido não encontrada.'

  function tentarSituacoesNovamente() {
    setErroSituacoes('')
    setTentativaSituacoes((anterior) => anterior + 1)
  }

  function tentarNovamente() {
    setTentativa((anterior) => anterior + 1)
  }

  function alterarFiltro(evento) {
    const valor = evento.target.value
    setParametros(valor ? { idSituacao: valor } : {})
  }

  // Troca o pedido atualizado na lista. A resposta do PUT não traz o nome do cliente: ele é mantido.
  function aoAtualizar(atualizado) {
    setResultado((atual) => ({
      ...atual,
      pedidos: atual.pedidos.map((pedido) => (pedido.id === atualizado.id ? { ...pedido, ...atualizado } : pedido)),
    }))
  }

  const nomeFiltro = situacoes.find((situacao) => String(situacao.id) === filtro)?.descricao

  return (
    <section>
      <h1 className="titulo-pagina">Pedidos</h1>
      <p className="subtitulo-pagina">Todos os pedidos da loja, do mais recente para o mais antigo.</p>

      <MensagemErro mensagem={erroSituacoes} aoTentarNovamente={tentarSituacoesNovamente} />

      <div className="painel mb-4 filtro-pedidos">
        <label htmlFor="filtro-situacao" className="form-label">Mostrar pedidos na situação</label>
        <select
          id="filtro-situacao"
          className="form-select"
          value={filtroValido ? filtro : ''}
          onChange={alterarFiltro}
          disabled={situacoes.length === 0}
        >
          <option value="">Todas as situações</option>
          {situacoes.map((situacao) => (
            <option key={situacao.id} value={situacao.id}>{situacao.descricao}</option>
          ))}
        </select>
      </div>

      {carregando && <Carregando texto="Carregando pedidos..." />}
      <MensagemErro mensagem={erro} aoTentarNovamente={tentarNovamente} />

      {!carregando && semSaida && (
        <EstadoVazio titulo={semSaida}>
          <p className="subtitulo-pagina">Escolha outra situação na lista acima ou veja todos os pedidos.</p>
          <Link to="/admin/pedidos" className="btn btn-primary">Ver todos os pedidos</Link>
        </EstadoVazio>
      )}

      {!carregando && !erro && !semSaida && pedidos.length === 0 && (
        <EstadoVazio titulo={filtro ? 'Nenhum pedido nesta situação' : 'Nenhum pedido ainda'}>
          <p className="subtitulo-pagina">
            {filtro
              ? 'Escolha outra situação na lista acima ou veja todos os pedidos.'
              : 'Os pedidos aparecem aqui assim que um cliente finaliza uma compra.'}
          </p>
          {filtro && <Link to="/admin/pedidos" className="btn btn-primary">Ver todos os pedidos</Link>}
        </EstadoVazio>
      )}

      {!carregando && !erro && !semSaida && pedidos.length > 0 && (
        <>
          <p className="small fw-semibold" aria-live="polite">
            {pedidos.length} {pedidos.length === 1 ? 'pedido' : 'pedidos'}
            {nomeFiltro ? ` na situação ${nomeFiltro}` : ''}
          </p>
          <ol className="list-unstyled row g-4 mb-0" aria-label="Pedidos">
            {pedidos.map((pedido) => (
              <li key={pedido.id} className="col-12 col-lg-6">
                <CartaoGestaoPedido pedido={pedido} situacoes={situacoes} aoAtualizar={aoAtualizar} />
              </li>
            ))}
          </ol>
        </>
      )}
    </section>
  )
}

// Situação do filtro que não existe (404, código 112): tentar de novo não adianta,
// então vai para semSaida; as demais falhas vão para erro, com "Tentar novamente".
function resultadoDaFalha(chave, falha) {
  const mensagem = obterMensagemErro(falha)
  return ehNaoEncontrado(falha)
    ? { chave, pedidos: [], erro: '', semSaida: mensagem }
    : { chave, pedidos: [], erro: mensagem, semSaida: '' }
}

function CartaoGestaoPedido({ pedido, situacoes, aoAtualizar }) {
  const [novaSituacao, setNovaSituacao] = useState(String(pedido.idSituacaoPedido))
  const [atualizando, setAtualizando] = useState(false)
  const [aviso, setAviso] = useState('')
  const [erro, setErro] = useState('')
  const idTitulo = `pedido-${pedido.id}`
  const idCampo = `situacao-pedido-${pedido.id}`
  const mesmaSituacao = novaSituacao === String(pedido.idSituacaoPedido)

  async function atualizarSituacao(evento) {
    evento.preventDefault()
    const idSituacao = Number(novaSituacao)
    // Cancelar é a troca de maior impacto: pede confirmação. O estoque não volta sozinho.
    if (
      idSituacao === ID_CANCELADO &&
      !window.confirm(
        `Marcar o pedido nº ${pedido.id} como Cancelado? O estoque dos jogos não é devolvido automaticamente.`,
      )
    ) {
      return
    }
    setAtualizando(true)
    setAviso('')
    setErro('')
    try {
      const atualizado = await pedidoApi.alterarSituacao(pedido.id, idSituacao)
      aoAtualizar(atualizado)
      setAviso(`Situação atualizada para ${atualizado.descricaoSituacao}.`)
    } catch (falha) {
      setErro(obterMensagemErro(falha))
    } finally {
      setAtualizando(false)
    }
  }

  return (
    <article className="pedido painel h-100" aria-labelledby={idTitulo}>
      <header className="pedido-cabecalho">
        <h2 id={idTitulo} className="h4 mb-0">Pedido nº {pedido.id}</h2>
        <SeloSituacao idSituacao={pedido.idSituacaoPedido} descricao={pedido.descricaoSituacao} />
      </header>
      <dl className="pedido-resumo">
        <div>
          <dt>Data</dt>
          <dd><time dateTime={pedido.data}>{formatarDataHora(pedido.data)}</time></dd>
        </div>
        <div>
          <dt>Cliente</dt>
          <dd className="text-break">{pedido.nomeUsuario}</dd>
        </div>
      </dl>
      <p className="pedido-itens-resumo">
        <span className="visually-hidden">Itens: </span>
        {pedido.itens.map((item) => `${item.quantidade} × ${item.nomeJogo}`).join(' · ')}
      </p>
      <p className="pedido-total">
        <span>Total</span>
        <span>{formatarPreco(pedido.total)}</span>
      </p>

      <form className="troca-situacao" onSubmit={atualizarSituacao}>
        <label htmlFor={idCampo} className="form-label">Nova situação</label>
        <div className="d-flex flex-wrap gap-2">
          <select
            id={idCampo}
            className="form-select troca-situacao-campo"
            value={novaSituacao}
            onChange={(evento) => {
              setNovaSituacao(evento.target.value)
              setAviso('')
              setErro('')
            }}
            disabled={atualizando || situacoes.length === 0}
          >
            {situacoes.map((situacao) => (
              <option key={situacao.id} value={situacao.id}>{situacao.descricao}</option>
            ))}
          </select>
          {/* Desabilitado enquanto grava e quando a situação escolhida já é a atual. */}
          <button type="submit" className="btn btn-primary" disabled={atualizando || mesmaSituacao}>
            {atualizando ? 'Atualizando...' : 'Atualizar situação'}
          </button>
        </div>
      </form>

      {/* Retorno da troca no próprio cartão, perto do botão que a disparou. */}
      <div role="status" aria-live="polite">
        {aviso && (
          <p className="estado estado-sucesso mt-3 mb-0 py-2">
            <span className="estado-icone" aria-hidden="true">✓</span>
            <span className="fw-semibold">{aviso}</span>
          </p>
        )}
      </div>
      {erro && (
        <div className="mt-3">
          <MensagemErro mensagem={erro} />
        </div>
      )}

      <Link
        to={`/pedidos/${pedido.id}`}
        className="btn btn-outline-primary align-self-start mt-3"
        aria-label={`Ver detalhes do pedido nº ${pedido.id}`}
      >
        Ver detalhes
      </Link>
    </article>
  )
}
