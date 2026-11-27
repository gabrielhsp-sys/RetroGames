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

// O usuário está logado, mas não pode ver o registro (HTTP 403): tentar de novo não adianta.
export function ehSemAcesso(erro) {
  return erro?.response?.status === 403
}

// Falha de validação (400, responseCode 3): { campo: [mensagens] } vira
// { campo: 'primeira mensagem' }, para mostrar embaixo de cada campo.
// Nos demais casos, {}.
export function obterErrosDosCampos(erro) {
  const envelope = erro?.response?.data
  if (envelope?.responseCode !== 3 || !envelope.data) return {}
  return Object.fromEntries(
    Object.entries(envelope.data).map(([campo, mensagens]) => [
      campo,
      Array.isArray(mensagens) ? mensagens[0] : String(mensagens),
    ]),
  )
}
