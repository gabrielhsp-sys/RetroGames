import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { jogoApi } from '../../api/servicos'
import { obterMensagemErro } from '../../api/erros'
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
    jogoApi
      .buscarPorId(id)
      .then(setJogo)
      // 404 (código 103): a API manda "Jogo não encontrado." no errorMessage.
      .catch((falha) => {
        setErro(obterMensagemErro(falha))
        setNaoEncontrado(falha.response?.status === 404)
      })
      .finally(() => setCarregando(false))
  }, [id])

  useEffect(() => {
    carregarJogo()
  }, [carregarJogo])

  return (
    <section>
      <Link to="/" className="d-inline-block mb-3">Voltar ao catálogo</Link>

      {carregando && <Carregando texto="Carregando jogo..." />}
      {/* Tentar de novo só ajuda se a falha for passageira; num 404, não. */}
      <MensagemErro mensagem={erro} aoTentarNovamente={naoEncontrado ? undefined : carregarJogo} />

      {!carregando && !erro && jogo && (
        <article>
          <h1 className="h3">{jogo.nome}</h1>
          <p className="text-body-secondary">{jogo.nomePlataforma}, {jogo.anoLancamento}</p>
          <p className="fs-4 fw-bold">{formatarPreco(jogo.preco)}</p>
          <dl className="row">
            <dt className="col-sm-3">Condição</dt>
            <dd className="col-sm-9">{jogo.descricaoCondicao}</dd>
            <dt className="col-sm-3">Estoque</dt>
            <dd className="col-sm-9">{jogo.estoque > 0 ? `${jogo.estoque} em estoque` : 'Esgotado'}</dd>
            <dt className="col-sm-3">Gêneros</dt>
            <dd className="col-sm-9">{jogo.generos.map((g) => g.nome).join(', ')}</dd>
          </dl>
          {/* Descrição é opcional: sem ela, a seção não aparece (nada de "null"). */}
          {jogo.descricao && <p>{jogo.descricao}</p>}
        </article>
      )}
    </section>
  )
}
