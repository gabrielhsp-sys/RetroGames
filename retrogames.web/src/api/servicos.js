import api from './api'

// Cada função devolve só o "data" do envelope { responseCode, data, errorMessage }.
const dados = (resposta) => resposta.data.data

export const jogoApi = {
  listar: (filtros) => api.get('/Jogo', { params: filtros }).then(dados),
  buscarPorId: (id) => api.get(`/Jogo/${id}`).then(dados),
}

export const plataformaApi = {
  listar: () => api.get('/Plataforma').then(dados),
}

export const generoApi = {
  listar: () => api.get('/Genero').then(dados),
}

export const condicaoJogoApi = {
  listar: () => api.get('/CondicaoJogo').then(dados),
}
