import { useEffect, useState } from 'react'
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAutenticacao } from '../hooks/useAutenticacao'
import { useCarrinho } from '../hooks/useCarrinho'

export default function Layout() {
  // Menu recolhido no celular: o React controla a classe "show" do Bootstrap,
  // e o menu fecha sozinho depois que o usuário escolhe um link.
  const [menuAberto, setMenuAberto] = useState(false)
  const fecharMenu = () => setMenuAberto(false)
  const { usuario, carregando, estaAutenticado, ehAdministrador, sair } = useAutenticacao()
  const { contador } = useCarrinho()
  const navigate = useNavigate()
  const local = useLocation()

  // Aviso de sucesso deixado por outra tela ao navegar (cadastro, pedido realizado):
  // navigate(destino, { state: { avisoGlobal: '...' } }). Vale só para a página de destino.
  const avisoRecebido = local.state?.avisoGlobal
  const [aviso, setAviso] = useState(null) // { texto, caminho }
  if (avisoRecebido && avisoRecebido !== aviso?.texto) {
    setAviso({ texto: avisoRecebido, caminho: local.pathname })
  } else if (aviso && aviso.caminho !== local.pathname) {
    setAviso(null) // saiu da página: o aviso não acompanha
  }

  // O aviso foi lido: apaga-o do histórico para não reaparecer no F5.
  useEffect(() => {
    if (local.state?.avisoGlobal) navigate(local.pathname + local.search, { replace: true, state: null })
  }, [local, navigate])

  async function sairDaLoja() {
    fecharMenu()
    await sair()
    navigate('/') // volta ao catálogo como visitante
  }

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
            {/* No celular o carrinho fica fora do menu recolhido: o contador está sempre à vista. */}
            <LinkCarrinho contador={contador} className="d-md-none ms-auto me-1" aoClicar={fecharMenu} />
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
                {/* C02: quem está logado (cliente ou administrador) vê os próprios pedidos. */}
                {estaAutenticado && (
                  <li className="nav-item">
                    <NavLink className="nav-link" to="/meus-pedidos" onClick={fecharMenu}>Meus pedidos</NavLink>
                  </li>
                )}
                {/* Menu Administração: só para o administrador. A API protege de verdade (RS03). */}
                {ehAdministrador && (
                  <li className="nav-item dropdown">
                    <button
                      type="button"
                      className="nav-link dropdown-toggle"
                      data-bs-toggle="dropdown"
                      aria-expanded="false"
                    >
                      Administração
                    </button>
                    <ul className="dropdown-menu">
                      <li><NavLink className="dropdown-item" to="/admin/jogos" onClick={fecharMenu}>Jogos</NavLink></li>
                      <li><NavLink className="dropdown-item" to="/admin/plataformas" onClick={fecharMenu}>Plataformas</NavLink></li>
                      <li><NavLink className="dropdown-item" to="/admin/pedidos" onClick={fecharMenu}>Pedidos</NavLink></li>
                    </ul>
                  </li>
                )}
              </ul>
              <div className="d-flex flex-wrap align-items-center gap-2 py-2 py-md-0">
                {/* O link aparece para todos, mas o carrinho exige login: o visitante entra para vê-lo e finalizar (C03). */}
                <LinkCarrinho contador={contador} className="d-none d-md-inline-flex" aoClicar={fecharMenu} />
                {/* Enquanto o "eu" não responde, não mostra Entrar nem Sair (evita piscar). */}
                {!carregando && (estaAutenticado ? (
                  <>
                    <span className="text-light">Olá, {usuario.nome}</span>
                    <button type="button" className="btn btn-barra" onClick={sairDaLoja}>Sair</button>
                  </>
                ) : (
                  <NavLink className="btn btn-barra" to="/login" onClick={fecharMenu}>Entrar</NavLink>
                ))}
              </div>
            </div>
          </div>
        </nav>
      </header>

      <main id="conteudo" className="container flex-grow-1 py-4 py-md-5" tabIndex={-1}>
        {/* A região "status" existe sempre: o leitor de tela anuncia o texto quando ele entra. */}
        <div role="status" aria-live="polite">
          {aviso && (
            <div className="estado estado-sucesso flex-wrap">
              <span className="estado-icone" aria-hidden="true">✓</span>
              <span className="me-auto fw-semibold">{aviso.texto}</span>
              <button type="button" className="btn btn-outline-primary" onClick={() => setAviso(null)}>
                Fechar aviso
              </button>
            </div>
          )}
        </div>
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

// Link do carrinho com o contador de unidades. O nome acessível diz o número
// ("Carrinho, 2 itens"); o selo repete o número para quem enxerga.
function LinkCarrinho({ contador, className, aoClicar }) {
  return (
    <NavLink
      className={`btn btn-barra carrinho-link ${className}`}
      to="/carrinho"
      onClick={aoClicar}
      aria-label={`Carrinho, ${contador} ${contador === 1 ? 'item' : 'itens'}`}
    >
      Carrinho
      <span className="contador" aria-hidden="true">{contador}</span>
    </NavLink>
  )
}
