import { createContext } from 'react'

// Os itens do carrinho, compartilhados com todas as telas (ver CarrinhoProvider).
export const CarrinhoContext = createContext(null)

// Chave do localStorage. Guarda só [{ idJogo, quantidade }]: não é sessão (RS01).
export const CHAVE_CARRINHO = 'retrogames.carrinho'

// Quantidade máxima de um jogo em um pedido (C03, C04).
export const QUANTIDADE_MAXIMA = 10

// Limite de um item: 10 ou o estoque do jogo, o que for menor.
export function limiteDoItem(estoque) {
  return Math.max(0, Math.min(QUANTIDADE_MAXIMA, estoque ?? QUANTIDADE_MAXIMA))
}
