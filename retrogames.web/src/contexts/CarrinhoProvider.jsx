import { useEffect, useState } from 'react'
import { CarrinhoContext, CHAVE_CARRINHO, limiteDoItem, QUANTIDADE_MAXIMA } from './carrinhoContext'

// O carrinho fica só no navegador (C03): a API só fica sabendo da compra ao finalizar.
// No localStorage vão apenas { idJogo, quantidade }. Nome, preço e estoque são
// buscados na API ao abrir o carrinho, e o preço nunca é guardado nem enviado.
// Isto não é sessão: quem está logado continua só no cookie HttpOnly (RS01).
// Lê e confere o que está guardado. JSON inválido, formato estranho ou storage
// bloqueado (modo privado, permissão) viram carrinho vazio, sem quebrar a tela.
function lerCarrinho() {
  try {
    const guardado = JSON.parse(window.localStorage.getItem(CHAVE_CARRINHO) ?? '[]')
    if (!Array.isArray(guardado)) return []
    const itens = []
    for (const item of guardado) {
      const valido = Number.isInteger(item?.idJogo) && item.idJogo > 0 && Number.isInteger(item.quantidade)
      // Cada jogo uma só vez, como o pedido exige.
      if (valido && !itens.some((jaLido) => jaLido.idJogo === item.idJogo)) {
        itens.push({ idJogo: item.idJogo, quantidade: Math.min(QUANTIDADE_MAXIMA, Math.max(1, item.quantidade)) })
      }
    }
    return itens
  } catch {
    return []
  }
}

function gravarCarrinho(itens) {
  try {
    window.localStorage.setItem(CHAVE_CARRINHO, JSON.stringify(itens))
  } catch {
    // Storage bloqueado ou cheio: o carrinho vale até fechar a aba.
  }
}

export default function CarrinhoProvider({ children }) {
  const [itens, setItens] = useState(lerCarrinho)

  // Outra aba mudou o carrinho: o evento "storage" avisa, e esta aba relê.
  useEffect(() => {
    function aoMudarEmOutraAba(evento) {
      if (evento.key === CHAVE_CARRINHO || evento.key === null) setItens(lerCarrinho())
    }
    window.addEventListener('storage', aoMudarEmOutraAba)
    return () => window.removeEventListener('storage', aoMudarEmOutraAba)
  }, [])

  function atualizar(novosItens) {
    gravarCarrinho(novosItens)
    setItens(novosItens)
  }

  // Soma ao que já está no carrinho, sem passar do limite. Devolve a quantidade
  // final e se o limite foi atingido, para a tela avisar.
  function adicionar(idJogo, estoque, quantidade = 1) {
    const limite = limiteDoItem(estoque)
    const atual = itens.find((item) => item.idJogo === idJogo)?.quantidade ?? 0
    const final = Math.min(limite, atual + quantidade)
    if (final > 0 && final !== atual) {
      atualizar(
        atual > 0
          ? itens.map((item) => (item.idJogo === idJogo ? { ...item, quantidade: final } : item))
          : [...itens, { idJogo, quantidade: final }],
      )
    }
    return { quantidade: final, adicionado: final - atual, atingiuLimite: final >= limite, limite }
  }

  function alterarQuantidade(idJogo, quantidade) {
    const corrigida = Math.min(QUANTIDADE_MAXIMA, Math.max(1, Math.trunc(quantidade) || 1))
    atualizar(itens.map((item) => (item.idJogo === idJogo ? { ...item, quantidade: corrigida } : item)))
  }

  function remover(idJogo) {
    atualizar(itens.filter((item) => item.idJogo !== idJogo))
  }

  function limpar() {
    atualizar([])
  }

  const valor = {
    itens,
    // Total de unidades: é o número do contador no menu.
    contador: itens.reduce((soma, item) => soma + item.quantidade, 0),
    adicionar,
    alterarQuantidade,
    remover,
    limpar,
  }

  return <CarrinhoContext.Provider value={valor}>{children}</CarrinhoContext.Provider>
}
