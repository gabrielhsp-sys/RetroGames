import { Link, NavLink, Outlet } from 'react-router-dom'

export default function Layout() {
  return (
    <div className="d-flex flex-column min-vh-100">
      <header>
        <nav className="navbar navbar-expand-md bg-body-tertiary border-bottom">
          <div className="container">
            <Link className="navbar-brand fw-bold" to="/">RetroGames</Link>
            <button
              className="navbar-toggler"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target="#menuPrincipal"
              aria-controls="menuPrincipal"
              aria-expanded="false"
              aria-label="Abrir o menu"
            >
              <span className="navbar-toggler-icon" />
            </button>
            <div className="collapse navbar-collapse" id="menuPrincipal">
              <ul className="navbar-nav me-auto">
                <li className="nav-item">
                  <NavLink className="nav-link" to="/" end>Catálogo</NavLink>
                </li>
              </ul>
            </div>
          </div>
        </nav>
      </header>

      <main className="container flex-grow-1 py-4">
        <Outlet />
      </main>

      <footer className="border-top py-3">
        <div className="container small">RetroGames · jogos retrô usados</div>
      </footer>
    </div>
  )
}
