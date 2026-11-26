import axios from 'axios'

// Única instância do Axios do projeto. Os componentes nunca importam o Axios:
// tudo passa por servicos.js. withCredentials faz o cookie de sessão ir junto.
const api = axios.create({ baseURL: '/api', withCredentials: true })

// 401 do "eu" (visitante) e do login (senha errada) é resposta esperada.
const ROTAS_401_ESPERADO = ['/Autenticacao/eu', '/Autenticacao/login']

// Um 401 em qualquer outra rota quer dizer que a sessão acabou: avisa o
// AutenticacaoProvider pelo evento "sessao:expirada" e repassa o erro à tela.
api.interceptors.response.use(
  (resposta) => resposta,
  (erro) => {
    const rota = erro.config?.url ?? ''
    if (erro.response?.status === 401 && !ROTAS_401_ESPERADO.includes(rota)) {
      window.dispatchEvent(new Event('sessao:expirada'))
    }
    return Promise.reject(erro)
  },
)

export default api
