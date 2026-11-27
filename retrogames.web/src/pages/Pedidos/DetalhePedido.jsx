import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { pedidoApi } from '../../api/servicos'
import { ehNaoEncontrado, ehSemAcesso, obterMensagemErro } from '../../api/erros'
import { Carregando, EstadoVazio, MensagemErro } from '../../components/MensagensEstado'
import { ItensPedido, SeloSituacao } from '../../components/Pedido'
import { useAutenticacao } from '../../hooks/useAutenticacao'
import { formatarDataHora, formatarPreco } from '../../lib/formato'

// C06: detalhe de um pedido (/pedidos/:id). O cliente vê só os seus; o administrador
// vê qualquer um. Quem decide é a API (403, código 113): a tela só mostra a resposta.
export default function DetalhePedido() {
  const { id } = useParams()
  const { ehAdministrador } = useAutenticacao()
  const idValido = /^\d+$/.test(id)

  const [pedido, setPedido] = useState(null)
  const [carregando, setCarregando] = useState(idValido)
  const [erro, setErro] = useState('')
  // 403 e 404 não melhoram tentando de novo: mostram a mensagem e a saída.
  const [semSaida, setSemSaida] = useState(idValido ? '' : 'Pedido não encontrado.')
  const [tentativa, setTentativa] = useState(0) // muda a cada "Tentar novamente"

  useEffect(() => {
    if (!idValido) return undefined
    let ativo = true // resposta que chega depois de sair da tela é ignorada
    pedidoApi
      .buscarPorId(id)
      .then((dados) => ativo && setPedido(dados))
      .catch((falha) => {
        if (!ativo) return
        if (ehSemAcesso(falha) || ehNaoEncontrado(falha)) setSemSaida(obterMensagemErro(falha))
        else setErro(obterMensagemErro(falha))
      })
      .finally(() => ativo && setCarregando(false))
    return () => {
      ativo = false
    }
  }, [id, idValido, tentativa])

  function tentarNovamente() {
    setCarregando(true)
    setErro('')
    setTentativa((anterior) => anterior + 1)
  }

  // O administrador chega pela gestão de pedidos; o cliente, por Meus pedidos.
  const voltar = ehAdministrador
    ? { para: '/admin/pedidos', texto: 'Voltar aos pedidos' }
    : { para: '/meus-pedidos', texto: 'Voltar para Meus pedidos' }

  const unidades = pedido?.itens.reduce((soma, item) => soma + item.quantidade, 0) ?? 0

  return (
    <section>
      {/* Em 403/404 a saída fica no próprio aviso: aqui não repete o mesmo botão. */}
      {!semSaida && (
        <Link to={voltar.para} className="btn btn-outline-primary gap-2 mb-3">
          <span aria-hidden="true">←</span>{voltar.texto}
        </Link>
      )}
      {/* Toda tela tem um h1: enquanto o cartão do pedido não aparece, o título fica aqui. */}
      {!pedido && <h1 className="titulo-pagina">{idValido ? `Pedido nº ${id}` : 'Pedido'}</h1>}

      {carregando && <Carregando texto="Carregando o pedido..." />}
      <MensagemErro mensagem={erro} aoTentarNovamente={tentarNovamente} />

      {semSaida && (
        <EstadoVazio titulo={semSaida}>
          <p className="subtitulo-pagina">
            {ehAdministrador
              ? 'Confira o número do pedido na lista de pedidos.'
              : 'Você pode ver todos os seus pedidos em Meus pedidos.'}
          </p>
          <Link to={voltar.para} className="btn btn-primary">{voltar.texto}</Link>
        </EstadoVazio>
      )}

      {!carregando && pedido && (
        <article className="pedido painel pedido-detalhe" aria-labelledby="titulo-pedido">
          <header className="pedido-cabecalho">
            <h1 id="titulo-pedido" className="titulo-pagina mb-0">Pedido nº {pedido.id}</h1>
            <SeloSituacao idSituacao={pedido.idSituacaoPedido} descricao={pedido.descricaoSituacao} />
          </header>
          <dl className="pedido-resumo">
            <div>
              <dt>Data</dt>
              <dd><time dateTime={pedido.data}>{formatarDataHora(pedido.data)}</time></dd>
            </div>
            <div>
              <dt>Itens</dt>
              <dd>{unidades} {unidades === 1 ? 'unidade' : 'unidades'}</dd>
            </div>
          </dl>

          <h2 className="h5">Itens do pedido</h2>
          <ItensPedido itens={pedido.itens} />

          <p className="pedido-total mb-0">
            <span>Total</span>
            <span>{formatarPreco(pedido.total)}</span>
          </p>
        </article>
      )}
    </section>
  )
}
