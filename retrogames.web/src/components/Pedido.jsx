import { formatarPreco } from '../lib/formato'

// Classe do selo por situação (tb_situacao_pedido). O texto da situação
// sempre aparece: a cor só reforça (WCAG 1.4.1).
const CLASSE_SITUACAO = {
  1: 'situacao-pendente',
  2: 'situacao-pago',
  3: 'situacao-enviado',
  4: 'situacao-cancelado',
}

// Selo com a situação do pedido. Usado em Meus pedidos, no detalhe e na gestão.
export function SeloSituacao({ idSituacao, descricao }) {
  return (
    <p className={`situacao ${CLASSE_SITUACAO[idSituacao] ?? ''} mb-0`}>
      <span className="visually-hidden">Situação: </span>{descricao}
    </p>
  )
}

// Itens do pedido: jogo, quantidade × preço pago e subtotal.
export function ItensPedido({ itens }) {
  return (
    <ul className="list-unstyled pedido-itens">
      {itens.map((item) => (
        <li key={item.idJogo}>
          <span className="pedido-item-nome">{item.nomeJogo}</span>
          <span className="pedido-item-conta">
            {item.quantidade} × {formatarPreco(item.valorUnitario)}
          </span>
          <span className="pedido-item-subtotal">{formatarPreco(item.quantidade * item.valorUnitario)}</span>
        </li>
      ))}
    </ul>
  )
}
