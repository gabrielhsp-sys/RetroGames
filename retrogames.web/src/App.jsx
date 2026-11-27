import { Navigate, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import RotaProtegida from './components/RotaProtegida'
import FormularioJogo from './pages/Administracao/FormularioJogo'
import Jogos from './pages/Administracao/Jogos'
import Plataforma from './pages/Auxiliares/Plataforma'
import Cadastro from './pages/Cadastro/Cadastro'
import Carrinho from './pages/Carrinho/Carrinho'
import Catalogo from './pages/Catalogo/Catalogo'
import DetalheJogo from './pages/Catalogo/DetalheJogo'
import Login from './pages/Login/Login'
import MeusPedidos from './pages/MeusPedidos/MeusPedidos'
import NaoEncontrada from './pages/NaoEncontrada'

export default function App() {
  return (
    <Routes>
      {/* Todas as páginas aparecem dentro do Layout, no lugar do <Outlet />. */}
      <Route element={<Layout />}>
        {/* Rotas públicas */}
        <Route index element={<Catalogo />} />
        <Route path="jogos/:id" element={<DetalheJogo />} />
        <Route path="login" element={<Login />} />
        <Route path="cadastro" element={<Cadastro />} />

        {/* Rotas de quem está logado (cliente ou administrador) */}
        <Route element={<RotaProtegida />}>
          <Route path="carrinho" element={<Carrinho />} />
          <Route path="meus-pedidos" element={<MeusPedidos />} />
        </Route>

        {/* Rotas restritas ao Administrador */}
        <Route path="admin" element={<RotaProtegida somenteAdministrador />}>
          <Route index element={<Navigate to="jogos" replace />} />
          <Route path="jogos" element={<Jogos />} />
          <Route path="jogos/novo" element={<FormularioJogo />} />
          <Route path="jogos/:id/editar" element={<FormularioJogo />} />
          <Route path="plataformas" element={<Plataforma />} />
        </Route>

        <Route path="*" element={<NaoEncontrada />} />
      </Route>
    </Routes>
  )
}
