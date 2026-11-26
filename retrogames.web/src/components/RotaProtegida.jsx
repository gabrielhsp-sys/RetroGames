import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAutenticacao } from '../hooks/useAutenticacao'
import { Carregando } from './MensagensEstado'

// Proteger a rota no front só melhora a navegação: quem protege de verdade é a
// API, que responde 401 ou 403 mesmo que alguém burle a tela (RS03).
export default function RotaProtegida({ somenteAdministrador = false }) {
  const { carregando, estaAutenticado, ehAdministrador } = useAutenticacao()
  const local = useLocation()

  // Espera o "eu" responder: assim o F5 numa tela de admin não manda ao login.
  if (carregando) return <Carregando texto="Verificando sessão..." />

  if (!estaAutenticado) {
    const voltarPara = encodeURIComponent(local.pathname + local.search)
    return <Navigate to={`/login?voltarPara=${voltarPara}`} replace />
  }

  if (somenteAdministrador && !ehAdministrador) return <Navigate to="/" replace />

  return <Outlet />
}
