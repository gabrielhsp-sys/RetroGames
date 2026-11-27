import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { pedidoApi } from '../../api/servicos'
import { obterMensagemErro } from '../../api/erros'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import { formatarDataHora, formatarPreco } from '../../lib/formato'

// Classe do selo por situação (tb_situacao_pedido). O texto da situação
// sempre aparece: a cor só reforça (WCAG 1.4.1).
const CLASSE_SITUACAO = {
  1: 'situacao-pendente',
  2: 'situacao-pago',
  3: 'situacao-enviado',
  4: 'situacao-cancelado',
}

// C05: os pedidos de quem está logado, do mais recente ao mais antigo (a API já ordena).
export default function MeusPedidos() {
  const [pedidos, setPedidos] = useState([])
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [tentativa, setTentativa] = useState(0) // muda a cada "Tentar novamente"

  useEffect(() => {
    let ativo = true // resposta que chega depois de sair da tela é ignorada
    pedidoApi
      .listarMeus()
      .then((lista) => ativo && setPedidos(lista))
      .catch((falha) => ativo && setErro(obterMensagemErro(falha)))
      .finally(() => ativo && setCarregando(false))
    return () => {
      ativo = false
    }
  }, [tentativa])

  function tentarNovamente() {
    setCarregando(true)
    setErro('')
    setTentativa((anterior) => anterior + 1)
  }

  return (
    <section>
      <h1 className="titulo-pagina">Meus pedidos</h1>
      <p className="subtitulo-pagina">Do mais recente para o mais antigo, com o preço que você pagou em cada jogo.</p>

      {carregando && <Carregando texto="Carregando seus pedidos..." />}
      <MensagemErro mensagem={erro} aoTentarNovamente={tentarNovamente} />

      {!carregando && !erro && pedidos.length === 0 && (
        <EstadoVazio titulo="Você ainda não comprou nada">
          <p className="subtitulo-pagina">Os pedidos que você finalizar aparecem aqui.</p>
          <Link to="/" className="btn btn-primary">Ver o catálogo</Link>
        </EstadoVazio>
      )}

      {!carregando && !erro && pedidos.length > 0 && (
        <ol className="list-unstyled row g-4 mb-0" aria-label="Pedidos">
          {pedidos.map((pedido) => (
            <li key={pedido.id} className="col-12 col-lg-6">
              <CartaoPedido pedido={pedido} />
            </li>
          ))}
        </ol>
      )}
    </section>
  )
}

function CartaoPedido({ pedido }) {
  const idTitulo = `pedido-${pedido.id}`
  return (
    <article className="pedido painel h-100" aria-labelledby={idTitulo}>
      <header className="pedido-cabecalho">
        <h2 id={idTitulo} className="h4 mb-0">Pedido nº {pedido.id}</h2>
        <p className={`situacao ${CLASSE_SITUACAO[pedido.idSituacaoPedido] ?? ''} mb-0`}>
          <span className="visually-hidden">Situação: </span>{pedido.descricaoSituacao}
        </p>
      </header>
      <p className="pedido-data">
        <span className="visually-hidden">Data: </span>
        <time dateTime={pedido.data}>{formatarDataHora(pedido.data)}</time>
      </p>

      <h3 className="visually-hidden">Itens</h3>
      <ul className="list-unstyled pedido-itens">
        {pedido.itens.map((item) => (
          <li key={item.idJogo}>
            <span className="pedido-item-nome">{item.nomeJogo}</span>
            <span className="pedido-item-conta">
              {item.quantidade} × {formatarPreco(item.valorUnitario)}
            </span>
            <span className="pedido-item-subtotal">{formatarPreco(item.quantidade * item.valorUnitario)}</span>
          </li>
        ))}
      </ul>

      <p className="pedido-total mb-0">
        <span>Total</span>
        <span>{formatarPreco(pedido.total)}</span>
      </p>
    </article>
  )
}
