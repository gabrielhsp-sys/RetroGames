import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { jogoApi, pedidoApi } from '../../api/servicos'
import { ehNaoEncontrado, obterMensagemErro } from '../../api/erros'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import { limiteDoItem } from '../../contexts/carrinhoContext'
import { useCarrinho } from '../../hooks/useCarrinho'
import { formatarPreco } from '../../lib/formato'

// Situação de cada item, a partir do jogo buscado na API (nunca do que está no navegador).
function situacaoDoItem(item, consulta) {
  if (!consulta || consulta.indisponivel || consulta.jogo.ativo === false) return { tipo: 'indisponivel' }
  const limite = limiteDoItem(consulta.jogo.estoque)
  if (limite === 0) return { tipo: 'esgotado' }
  if (item.quantidade > limite) return { tipo: 'acimaDoEstoque', limite }
  return { tipo: 'ok', limite }
}

const JOGO_INDISPONIVEL = 110
const ESTOQUE_INSUFICIENTE = 111

// Depois de um 110 ou 111, diz pelo nome o que mudou (heurística 9).
function descreverPendencias(linhas) {
  return linhas
    .map(({ jogo, situacao }) => {
      if (situacao.tipo === 'acimaDoEstoque') {
        return `Estoque insuficiente para ${jogo.nome}: ${situacao.limite === 1 ? 'resta 1' : `restam ${situacao.limite}`}.`
      }
      if (situacao.tipo === 'esgotado') return `${jogo.nome} esgotou.`
      if (situacao.tipo === 'indisponivel') return `${jogo?.nome ?? 'Um dos jogos'} não está mais à venda.`
      return null
    })
    .filter(Boolean)
    .join(' ')
}

// C03: o carrinho. O navegador guarda só { idJogo, quantidade }; nome, plataforma,
// preço e estoque vêm da API cada vez que a tela abre, então o total mostrado é
// sempre o preço atual do banco.
export default function Carrinho() {
  const { itens, alterarQuantidade, remover, adicionar, limpar } = useCarrinho()
  const navigate = useNavigate()
  // { [idJogo]: { jogo } | { indisponivel: true } }: o que a API respondeu sobre cada jogo.
  const [consultas, setConsultas] = useState({})
  const [erroCarga, setErroCarga] = useState('')
  const [removido, setRemovido] = useState(null) // último item removido, para "Desfazer"
  const [enviando, setEnviando] = useState(false)
  // Falha ao finalizar: { tipo: 'mudou' } (110/111) ou { tipo: 'outro', mensagem }.
  const [erroCompra, setErroCompra] = useState(null)

  // Jogos do carrinho que ainda não foram consultados (ao abrir, ou vindos de outra aba).
  const faltando = itens.map((item) => item.idJogo).filter((id) => !(id in consultas))
  const chaveFaltando = faltando.join(',')
  const carregando = faltando.length > 0 && !erroCarga

  useEffect(() => {
    if (!chaveFaltando || erroCarga) return
    const ids = chaveFaltando.split(',').map(Number)
    Promise.allSettled(ids.map((id) => jogoApi.buscarPorId(id))).then((respostas) => {
      const novas = {}
      let falhaDeRede = null
      respostas.forEach((resposta, i) => {
        if (resposta.status === 'fulfilled') novas[ids[i]] = { jogo: resposta.value }
        // 404: o jogo saiu do catálogo (inativo ou excluído).
        else if (ehNaoEncontrado(resposta.reason)) novas[ids[i]] = { indisponivel: true }
        else falhaDeRede = resposta.reason
      })
      setConsultas((anteriores) => ({ ...anteriores, ...novas }))
      if (falhaDeRede) setErroCarga(obterMensagemErro(falhaDeRede))
    })
  }, [chaveFaltando, erroCarga])

  function tentarNovamente() {
    setErroCarga('') // o efeito volta a buscar o que falta
  }

  // C04: envia só { idJogo, quantidade }. Preço e total quem calcula é a API, com o preço do banco.
  async function finalizarCompra() {
    setEnviando(true)
    setErroCompra(null)
    setRemovido(null)
    try {
      const pedido = await pedidoApi.criar(itens.map(({ idJogo, quantidade }) => ({ idJogo, quantidade })))
      limpar()
      navigate('/meus-pedidos', {
        state: { avisoGlobal: `Pedido nº ${pedido.id} realizado. Total: ${formatarPreco(pedido.total)}.` },
      })
    } catch (falha) {
      setEnviando(false)
      // 401: o interceptor avisa que a sessão acabou e a RotaProtegida leva ao
      // Login com voltarPara=/carrinho; o carrinho continua no navegador.
      if (falha?.response?.status === 401) return
      const codigo = falha?.response?.data?.responseCode
      if (codigo === JOGO_INDISPONIVEL || codigo === ESTOQUE_INSUFICIENTE) {
        // Algo mudou desde que o jogo entrou no carrinho: busca tudo de novo
        // para marcar, pelo nome, o que ficou indisponível ou com menos estoque.
        setConsultas({})
        setErroCompra({ tipo: 'mudou', mensagem: obterMensagemErro(falha) })
      } else {
        // Rede, servidor fora do ar ou erro inesperado: o mesmo carrinho pode ser reenviado.
        setErroCompra({ tipo: 'outro', mensagem: obterMensagemErro(falha) })
      }
    }
  }

  function removerItem(item, nome) {
    remover(item.idJogo)
    setRemovido({ ...item, nome })
  }

  function desfazerRemocao() {
    const consulta = consultas[removido.idJogo]
    adicionar(removido.idJogo, consulta?.jogo?.estoque, removido.quantidade)
    setRemovido(null)
  }

  const linhas = itens.map((item) => {
    const consulta = consultas[item.idJogo]
    return { item, jogo: consulta?.jogo, situacao: situacaoDoItem(item, consulta) }
  })
  const disponiveis = linhas.filter((linha) => linha.situacao.tipo === 'ok')
  const total = disponiveis.reduce((soma, linha) => soma + linha.jogo.preco * linha.item.quantidade, 0)
  const unidades = disponiveis.reduce((soma, linha) => soma + linha.item.quantidade, 0)
  const pendencias = linhas.length - disponiveis.length
  const podeFinalizar = itens.length > 0 && pendencias === 0 && !enviando

  // Mensagem da falha ao finalizar. Em 110/111, nomeia os itens que mudaram; se,
  // ao recarregar, nada mudou (o estoque voltou), pede para tentar de novo.
  let mensagemCompra = ''
  if (erroCompra?.tipo === 'mudou' && !carregando) {
    mensagemCompra = descreverPendencias(linhas) || `${erroCompra.mensagem} Confira o carrinho e tente de novo.`
  } else if (erroCompra?.tipo === 'outro') {
    mensagemCompra = erroCompra.mensagem
  }

  return (
    <section>
      <Link to="/" className="d-inline-flex align-items-center mb-3 fw-semibold link-voltar">
        <span aria-hidden="true">←</span>&nbsp;Continuar comprando
      </Link>
      <h1 className="titulo-pagina">Carrinho</h1>
      <p className="subtitulo-pagina">Confira os jogos e as quantidades antes de finalizar a compra.</p>

      <div role="status" aria-live="polite">
        {removido && (
          <div className="estado flex-wrap">
            <span className="me-auto fw-semibold">{removido.nome} removido do carrinho.</span>
            <button type="button" className="btn btn-outline-primary" onClick={desfazerRemocao}>Desfazer</button>
          </div>
        )}
      </div>

      {itens.length === 0 && (
        <EstadoVazio titulo="Seu carrinho está vazio">
          <p className="subtitulo-pagina">Escolha um jogo no catálogo e use o botão "Adicionar ao carrinho".</p>
          <Link to="/" className="btn btn-primary">Ver o catálogo</Link>
        </EstadoVazio>
      )}

      {carregando && <Carregando texto="Carregando o carrinho..." />}
      <MensagemErro mensagem={erroCarga} aoTentarNovamente={tentarNovamente} />
      {/* Em 110/111 o cliente ajusta o carrinho; nas outras falhas, pode reenviar o mesmo pedido. */}
      <MensagemErro
        mensagem={mensagemCompra}
        aoTentarNovamente={erroCompra?.tipo === 'outro' && podeFinalizar ? finalizarCompra : undefined}
      />

      {itens.length > 0 && !carregando && !erroCarga && (
        <div className="row g-4 align-items-start">
          <div className="col-12 col-lg-8">
            <ul className="list-unstyled d-grid gap-3 mb-0" aria-label="Itens do carrinho">
              {linhas.map(({ item, jogo, situacao }) => (
                <LinhaCarrinho
                  key={item.idJogo}
                  item={item}
                  jogo={jogo}
                  situacao={situacao}
                  aoMudarQuantidade={(quantidade) => alterarQuantidade(item.idJogo, quantidade)}
                  aoRemover={() => removerItem(item, jogo?.nome ?? 'Jogo indisponível')}
                />
              ))}
            </ul>
          </div>
          <aside className="col-12 col-lg-4" aria-labelledby="titulo-resumo">
            <div className="painel resumo p-4">
              <h2 id="titulo-resumo" className="h4 mb-3">Resumo do pedido</h2>
              {/* O que vai ser comprado, à vista antes de confirmar (heurística 6). */}
              <ul className="list-unstyled resumo-itens mb-3">
                {disponiveis.map(({ item, jogo }) => (
                  <li key={item.idJogo}>
                    <span>{item.quantidade} × {jogo.nome}</span>
                    <span>{formatarPreco(jogo.preco * item.quantidade)}</span>
                  </li>
                ))}
              </ul>
              <dl className="resumo-linhas mb-3">
                <dt>Itens</dt>
                <dd>{unidades} {unidades === 1 ? 'unidade' : 'unidades'}</dd>
                <dt className="resumo-total">Total</dt>
                <dd className="resumo-total">{formatarPreco(total)}</dd>
              </dl>
              {pendencias > 0 && (
                <p id="aviso-pendencia" className="aviso-pendencia">
                  {pendencias === 1 ? '1 item precisa' : `${pendencias} itens precisam`} de ajuste antes da compra.
                  O total não inclui {pendencias === 1 ? 'esse item' : 'esses itens'}.
                </p>
              )}
              {/* Desabilitado durante o envio (evita pedido duplo) e enquanto houver item a ajustar. */}
              <button
                type="button"
                className="btn btn-primary btn-lg w-100"
                onClick={finalizarCompra}
                disabled={!podeFinalizar}
                aria-describedby={pendencias > 0 ? 'aviso-pendencia' : undefined}
              >
                {enviando ? 'Finalizando...' : 'Finalizar compra'}
              </button>
              <p className="small text-body-secondary mt-2 mb-0">
                O pedido começa como Pendente. Os preços são os da loja no momento da compra.
              </p>
            </div>
          </aside>
        </div>
      )}
    </section>
  )
}

// Uma linha do carrinho, no visual do cartucho: rótulo com plataforma e nome,
// etiqueta laranja com o preço, quantidade e remover.
function LinhaCarrinho({ item, jogo, situacao, aoMudarQuantidade, aoRemover }) {
  const idQuantidade = `quantidade-${item.idJogo}`
  const idAviso = `aviso-${item.idJogo}`

  if (situacao.tipo === 'indisponivel') {
    return (
      <li className="linha-carrinho linha-indisponivel">
        <MiniCartucho />
        <div className="linha-info">
          <p className="linha-nome mb-1">{jogo?.nome ?? 'Jogo indisponível'}</p>
          <p id={idAviso} className="linha-aviso mb-0">
            Este jogo não está mais à venda. Remova-o para finalizar a compra.
          </p>
        </div>
        <div className="linha-acoes">
          <button type="button" className="btn btn-outline-danger" onClick={aoRemover} aria-describedby={idAviso}>
            Remover
          </button>
        </div>
      </li>
    )
  }

  const precisaAjuste = situacao.tipo !== 'ok'

  // O botão "Ajustar" some depois do clique: o foco vai para a quantidade, não se perde.
  function ajustar(quantidade) {
    aoMudarQuantidade(quantidade)
    window.requestAnimationFrame(() => document.getElementById(idQuantidade)?.focus())
  }
  // As opções vão de 1 ao limite (heurística 5); se a quantidade passou do
  // estoque, ela aparece marcada para o cliente ver o que precisa mudar.
  const opcoes = Array.from({ length: situacao.limite ?? 0 }, (_, i) => i + 1)

  return (
    <li className={`linha-carrinho ${precisaAjuste ? 'linha-indisponivel' : ''}`}>
      <MiniCartucho />
      <div className="linha-info">
        <p className="linha-plataforma mb-0">{jogo.nomePlataforma}</p>
        <p className="linha-nome mb-1">
          <Link to={`/jogos/${jogo.id}`}>{jogo.nome}</Link>
        </p>
        <p className="etiqueta etiqueta-linha mb-0">
          <span className="visually-hidden">Preço unitário: </span>{formatarPreco(jogo.preco)}
        </p>
        {situacao.tipo === 'esgotado' && (
          <p id={idAviso} className="linha-aviso mb-0 mt-2">Esgotado. Remova-o para finalizar a compra.</p>
        )}
        {situacao.tipo === 'acimaDoEstoque' && (
          <p id={idAviso} className="linha-aviso mb-0 mt-2">
            Estoque insuficiente para {jogo.nome}: {situacao.limite === 1 ? 'resta 1' : `restam ${situacao.limite}`}.{' '}
            <button type="button" className="btn btn-link p-0 align-baseline fw-bold" onClick={() => ajustar(situacao.limite)}>
              Ajustar para {situacao.limite}
            </button>
          </p>
        )}
      </div>
      <div className="linha-acoes">
        {situacao.tipo !== 'esgotado' && (
          <div>
            <label htmlFor={idQuantidade} className="form-label small mb-1">Quantidade</label>
            <select
              id={idQuantidade}
              className={`form-select ${situacao.tipo === 'acimaDoEstoque' ? 'is-invalid' : ''}`}
              value={item.quantidade}
              onChange={(evento) => aoMudarQuantidade(Number(evento.target.value))}
              aria-describedby={precisaAjuste ? idAviso : undefined}
            >
              {opcoes.map((n) => <option key={n} value={n}>{n}</option>)}
              {situacao.tipo === 'acimaDoEstoque' && (
                <option value={item.quantidade} disabled>{item.quantidade} (acima do estoque)</option>
              )}
            </select>
          </div>
        )}
        {situacao.tipo === 'ok' && (
          <p className="linha-subtotal mb-0">
            <span className="small d-block">Subtotal</span>
            {formatarPreco(jogo.preco * item.quantidade)}
          </p>
        )}
        <button type="button" className="btn btn-outline-danger" onClick={aoRemover}>
          Remover<span className="visually-hidden"> {jogo.nome}</span>
        </button>
      </div>
    </li>
  )
}

// Cartucho pequeno, só de enfeite: a informação está no texto ao lado.
function MiniCartucho() {
  return (
    <div className="cartucho cartucho-mini" aria-hidden="true">
      <div className="cartucho-casca" />
      <div className="cartucho-rotulo" />
    </div>
  )
}
