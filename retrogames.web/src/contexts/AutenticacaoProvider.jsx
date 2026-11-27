import { useEffect, useState } from 'react'
import { autenticacaoApi } from '../api/servicos'
import { AutenticacaoContext } from './autenticacaoContext'

// O cookie de sessão é HttpOnly: o JavaScript não o lê. Para saber quem está
// logado, perguntamos à API (GET /api/Autenticacao/eu) ao abrir o site e a cada F5.
// Nada é guardado no localStorage nem no sessionStorage.
export default function AutenticacaoProvider({ children }) {
  const [usuario, setUsuario] = useState(null)
  const [carregando, setCarregando] = useState(true)

  useEffect(() => {
    autenticacaoApi
      .usuarioAtual()
      .then(setUsuario)
      .catch(() => setUsuario(null)) // 401: visitante, e isso é normal
      .finally(() => setCarregando(false))
  }, [])

  // O interceptor do api.js dispara este evento quando a sessão expira.
  useEffect(() => {
    const limparUsuario = () => setUsuario(null)
    window.addEventListener('sessao:expirada', limparUsuario)
    return () => window.removeEventListener('sessao:expirada', limparUsuario)
  }, [])

  async function entrar(email, senha) {
    const usuarioLogado = await autenticacaoApi.entrar(email, senha)
    setUsuario(usuarioLogado)
    return usuarioLogado
  }

  // C01: a API cria a conta e já inicia a sessão; o menu muda sem F5.
  async function cadastrar(nome, email, senha) {
    const usuarioNovo = await autenticacaoApi.cadastrar(nome, email, senha)
    setUsuario(usuarioNovo)
    return usuarioNovo
  }

  async function sair() {
    try {
      await autenticacaoApi.sair()
    } finally {
      setUsuario(null)
    }
  }

  const valor = {
    usuario,
    carregando,
    estaAutenticado: usuario !== null,
    ehAdministrador: usuario?.perfil === 'Administrador',
    entrar,
    cadastrar,
    sair,
  }

  return <AutenticacaoContext.Provider value={valor}>{children}</AutenticacaoContext.Provider>
}
