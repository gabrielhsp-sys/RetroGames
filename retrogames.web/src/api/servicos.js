import api from './api'

// Cada função devolve só o "data" do envelope { responseCode, data, errorMessage }.
const dados = (resposta) => resposta.data.data

export const autenticacaoApi = {
  entrar: (email, senha) => api.post('/Autenticacao/login', { email, senha }).then(dados),
  sair: () => api.post('/Autenticacao/logout').then(dados),
  usuarioAtual: () => api.get('/Autenticacao/eu').then(dados),
  // C01: cria a conta de cliente; a API já inicia a sessão (cookie) e devolve o mesmo modelo do "eu".
  cadastrar: (nome, email, senha) => api.post('/Autenticacao/cadastrar', { nome, email, senha }).then(dados),
}

export const jogoApi = {
  listar: (filtros) => api.get('/Jogo', { params: filtros }).then(dados),
  buscarPorId: (id) => api.get(`/Jogo/${id}`).then(dados),
  inserir: (jogo) => api.post('/Jogo', jogo).then(dados),
  alterar: (id, jogo) => api.put(`/Jogo/${id}`, jogo).then(dados),
  desativar: (id) => api.delete(`/Jogo/${id}`).then(dados), // exclusão lógica (RN03)
}

export const plataformaApi = {
  listar: () => api.get('/Plataforma').then(dados),
  inserir: (plataforma) => api.post('/Plataforma', plataforma).then(dados),
  alterar: (id, plataforma) => api.put(`/Plataforma/${id}`, plataforma).then(dados),
  excluir: (id) => api.delete(`/Plataforma/${id}`).then(dados),
}

export const generoApi = {
  listar: () => api.get('/Genero').then(dados),
  // C08 (administrador): mesma manutenção das plataformas.
  inserir: (genero) => api.post('/Genero', genero).then(dados),
  alterar: (id, genero) => api.put(`/Genero/${id}`, genero).then(dados),
  excluir: (id) => api.delete(`/Genero/${id}`).then(dados),
}

export const condicaoJogoApi = {
  listar: () => api.get('/CondicaoJogo').then(dados),
}

// Módulo de Compras. O corpo do pedido leva só idJogo e quantidade:
// o preço vem sempre do banco, nunca do front (C04).
export const pedidoApi = {
  criar: (itens) => api.post('/Pedido', { itens }).then(dados),
  listarMeus: () => api.get('/Pedido/meus').then(dados),
  // C06: o dono ou o administrador; pedido de outro cliente responde 403 (código 113).
  buscarPorId: (id) => api.get(`/Pedido/${id}`).then(dados),
  // C07 (administrador): todos os pedidos, com filtro opcional, e a troca de situação.
  listar: (idSituacao) => api.get('/Pedido', { params: { idSituacao } }).then(dados),
  alterarSituacao: (id, idSituacao) => api.put(`/Pedido/${id}/situacao`, { idSituacao }).then(dados),
}

export const situacaoPedidoApi = {
  listar: () => api.get('/SituacaoPedido').then(dados), // só administrador (C07)
}
