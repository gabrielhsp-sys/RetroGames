import { Link } from 'react-router-dom'

export default function NaoEncontrada() {
  return (
    <section className="text-center py-4 nao-encontrada">
      <div className="cartucho cartucho-vazio" aria-hidden="true">
        <div className="cartucho-casca" />
        <div className="cartucho-rotulo" />
      </div>
      <h1 className="titulo-pagina">Página não encontrada</h1>
      <p className="subtitulo-pagina">Este endereço não existe na loja. Os jogos estão todos no catálogo.</p>
      <Link to="/" className="btn btn-primary">Voltar ao catálogo</Link>
    </section>
  )
}
