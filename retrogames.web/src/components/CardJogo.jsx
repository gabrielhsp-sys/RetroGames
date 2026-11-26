import { Link } from 'react-router-dom'
import { formatarPreco } from '../lib/formato'

// Só desenha o jogo que recebe por props: não busca nada nem guarda estado.
// O cartão é um cartucho: rótulo com plataforma, nome, ano e condição,
// etiqueta laranja com o preço e, se for o caso, o carimbo de esgotado.
export default function CardJogo({ jogo }) {
  const esgotado = jogo.estoque === 0
  return (
    <article className="jogo">
      <div className="cartucho">
        <div className="cartucho-casca" aria-hidden="true" />
        <div className="cartucho-rotulo">
          <p className="cartucho-plataforma">{jogo.nomePlataforma}</p>
          <h2 className="cartucho-nome">
            <Link to={`/jogos/${jogo.id}`} className="stretched-link">{jogo.nome}</Link>
          </h2>
          <p className="cartucho-meta">{jogo.anoLancamento} · {jogo.descricaoCondicao}</p>
        </div>
        <p className="etiqueta">
          <span className="visually-hidden">Preço: </span>{formatarPreco(jogo.preco)}
        </p>
        {esgotado && <p className="carimbo">Esgotado</p>}
      </div>
      <p className="jogo-generos">{jogo.generos.join(', ')}</p>
    </article>
  )
}
