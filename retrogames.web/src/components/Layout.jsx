import { useState } from 'react'
import { Link, NavLink, Outlet } from 'react-router-dom'

export default function Layout() {
  // Menu recolhido no celular: o React controla a classe "show" do Bootstrap,
  // e o menu fecha sozinho depois que o usuário escolhe um link.
  const [menuAberto, setMenuAberto] = useState(false)
  const fecharMenu = () => setMenuAberto(false)

  return (
    <div className="d-flex flex-column min-vh-100">
      <a href="#conteudo" className="visually-hidden-focusable btn btn-primary position-absolute m-2">
        Pular para o conteúdo
      </a>
      <header>
        <nav className="navbar navbar-expand-md barra" data-bs-theme="dark" aria-label="Menu principal">
          <div className="container">
            <Link className="navbar-brand" to="/" onClick={fecharMenu}>
              <img src="/favicon.svg" alt="" width="36" height="36" />
              <span className="marca-texto">RetroGames</span>
            </Link>
            <button
              className="navbar-toggler"
              type="button"
              aria-controls="menuPrincipal"
              aria-expanded={menuAberto}
              aria-label={menuAberto ? 'Fechar o menu' : 'Abrir o menu'}
              onClick={() => setMenuAberto(!menuAberto)}
            >
              <span className="navbar-toggler-icon" />
            </button>
            <div className={`collapse navbar-collapse ${menuAberto ? 'show' : ''}`} id="menuPrincipal">
              <ul className="navbar-nav me-auto">
                <li className="nav-item">
                  <NavLink className="nav-link" to="/" end onClick={fecharMenu}>Catálogo</NavLink>
                </li>
              </ul>
            </div>
          </div>
        </nav>
      </header>

      <main id="conteudo" className="container flex-grow-1 py-4 py-md-5" tabIndex={-1}>
        <Outlet />
      </main>

      <footer className="rodape py-4">
        <div className="container d-flex flex-wrap align-items-center gap-2 small">
          <img src="/favicon.svg" alt="" width="24" height="24" />
          <span>RetroGames · cartuchos e jogos retrô usados, conferidos antes de ir para a prateleira.</span>
        </div>
      </footer>
    </div>
  )
}
