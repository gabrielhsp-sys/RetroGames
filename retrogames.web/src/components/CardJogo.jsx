import { Link } from 'react-router-dom'
import { formatarPreco } from '../lib/formato'

// Só desenha o jogo que recebe por props: não busca nada nem guarda estado.
export default function CardJogo({ jogo }) {
  const esgotado = jogo.estoque === 0
  return (
    <article className="card h-100 position-relative">
      <div className="card-body d-flex flex-column">
        <p className="small text-body-secondary mb-1">{jogo.nomePlataforma}</p>
        <h2 className="h6 card-title">
          <Link to={`/jogos/${jogo.id}`} className="stretched-link text-reset text-decoration-none">
            {jogo.nome}
          </Link>
        </h2>
        <p className="small mb-1">{jogo.anoLancamento} · {jogo.descricaoCondicao}</p>
        <p className="small text-body-secondary">{jogo.generos.join(', ')}</p>
        <div className="mt-auto d-flex align-items-center justify-content-between">
          <strong>{formatarPreco(jogo.preco)}</strong>
          {esgotado && <span className="badge text-bg-secondary">Esgotado</span>}
        </div>
      </div>
    </article>
  )
}
