import { useContext } from 'react'
import { AutenticacaoContext } from '../contexts/autenticacaoContext'

// Dá às telas acesso a usuario, carregando, estaAutenticado, ehAdministrador, entrar e sair.
export function useAutenticacao() {
  return useContext(AutenticacaoContext)
}
