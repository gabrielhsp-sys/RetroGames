import { Link } from 'react-router-dom'

export default function NaoEncontrada() {
  return (
    <section className="text-center py-5">
      <h1 className="h3">Página não encontrada</h1>
      <p>O endereço que você abriu não existe na loja.</p>
      <Link to="/" className="btn btn-primary">Voltar ao catálogo</Link>
    </section>
  )
}
