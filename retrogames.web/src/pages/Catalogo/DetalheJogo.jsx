import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { jogoApi } from '../../api/servicos'
import { ehNaoEncontrado, obterMensagemErro } from '../../api/erros'
import { formatarPreco } from '../../lib/formato'
import { Carregando, MensagemErro } from '../../components/MensagensEstado'

export default function DetalheJogo() {
  const { id } = useParams() // /jogos/3 → id = "3"
  const [jogo, setJogo] = useState(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')
  const [naoEncontrado, setNaoEncontrado] = useState(false)

  const carregarJogo = useCallback(() => {
    setCarregando(true)
    setErro('')
    setNaoEncontrado(false)
    // A rota da API só aceita número: /jogos/abc nem chega a ser consultado.
    if (!/^\d+$/.test(id)) {
      setErro('Jogo não encontrado.')
      setNaoEncontrado(true)
      setCarregando(false)
      return
    }
    jogoApi
      .buscarPorId(id)
      .then(setJogo)
      // 404 (código 103): a API manda "Jogo não encontrado." no errorMessage.
      .catch((falha) => {
        setErro(obterMensagemErro(falha))
        setNaoEncontrado(ehNaoEncontrado(falha))
      })
      .finally(() => setCarregando(false))
  }, [id])

  useEffect(() => {
    carregarJogo()
  }, [carregarJogo])

  return (
    <section>
      <Link to="/" className="d-inline-flex align-items-center mb-4 fw-semibold link-voltar">
        <span aria-hidden="true">←</span>&nbsp;Voltar ao catálogo
      </Link>

      {carregando && <Carregando texto="Carregando jogo..." />}
      {/* Tentar de novo só ajuda se a falha for passageira; num 404, não. */}
      <MensagemErro mensagem={erro} aoTentarNovamente={naoEncontrado ? undefined : carregarJogo} />

      {!carregando && !erro && jogo && (
        <article className="row g-4 g-lg-5 align-items-start">
          <div className="col-12 col-md-5">
            {/* O cartucho grande é a imagem do produto. */}
            <div className="cartucho cartucho-grande" aria-hidden="true">
              <div className="cartucho-casca" />
              <div className="cartucho-rotulo">
                <p className="cartucho-plataforma">{jogo.nomePlataforma}</p>
                <p className="cartucho-nome">{jogo.nome}</p>
                <p className="cartucho-meta">{jogo.anoLancamento}</p>
              </div>
              {jogo.estoque === 0 && <p className="carimbo">Esgotado</p>}
            </div>
          </div>
          <div className="col-12 col-md-7">
            <h1 className="titulo-pagina">{jogo.nome}</h1>
            <p className="subtitulo-pagina">{jogo.nomePlataforma}, {jogo.anoLancamento}</p>
            <p className="etiqueta etiqueta-grande mb-4">
              <span className="visually-hidden">Preço: </span>{formatarPreco(jogo.preco)}
            </p>
            <dl className="row painel ficha mx-0 mb-4">
              <dt className="col-5 col-sm-4">Condição</dt>
              <dd className="col-7 col-sm-8">{jogo.descricaoCondicao}</dd>
              <dt className="col-5 col-sm-4">Estoque</dt>
              <dd className="col-7 col-sm-8">{jogo.estoque > 0 ? `${jogo.estoque} em estoque` : 'Esgotado'}</dd>
              <dt className="col-5 col-sm-4">Gêneros</dt>
              <dd className="col-7 col-sm-8 mb-0">{jogo.generos.map((g) => g.nome).join(', ')}</dd>
            </dl>
            {/* Descrição é opcional: sem ela, a seção não aparece (nada de "null"). */}
            {jogo.descricao && <p className="descricao">{jogo.descricao}</p>}
          </div>
        </article>
      )}
    </section>
  )
}
