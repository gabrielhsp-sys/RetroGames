import { useContext } from 'react'
import { CarrinhoContext } from '../contexts/carrinhoContext'

// Dá às telas acesso a itens, contador, adicionar, alterarQuantidade, remover e limpar.
export function useCarrinho() {
  return useContext(CarrinhoContext)
}
