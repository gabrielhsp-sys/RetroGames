// Transforma a falha de uma chamada à API em mensagem para o usuário.
export function obterMensagemErro(erro) {
  const envelope = erro?.response?.data
  // Sem resposta, ou com uma resposta que não é o envelope da API (o proxy do
  // Vite devolve 500 sem corpo quando a API está parada): o servidor não respondeu.
  if (!erro?.response || typeof envelope?.responseCode !== 'number') {
    return 'Não foi possível conectar ao servidor.'
  }
  return envelope.errorMessage || 'Ocorreu um erro inesperado.'
}

// O registro pedido não existe (HTTP 404): tentar de novo não adianta.
export function ehNaoEncontrado(erro) {
  return erro?.response?.status === 404
}
