// Só aceita destino interno ("/carrinho"), nunca outro site ("//exemplo.com").
// Usado pelo Login e pelo Cadastro para voltar à tela de onde o usuário veio.
export function destinoSeguro(voltarPara) {
  return voltarPara && voltarPara.startsWith('/') && !voltarPara.startsWith('//') ? voltarPara : '/'
}

// Repassa o voltarPara de uma tela para a outra ("/cadastro?voltarPara=%2Fcarrinho").
export function comVoltarPara(caminho, voltarPara) {
  return voltarPara ? `${caminho}?voltarPara=${encodeURIComponent(voltarPara)}` : caminho
}
