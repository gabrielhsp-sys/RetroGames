import axios from 'axios'

// Única instância do Axios do projeto. Os componentes nunca importam o Axios:
// tudo passa por servicos.js. withCredentials faz o cookie de sessão ir junto.
const api = axios.create({ baseURL: '/api', withCredentials: true })

export default api
