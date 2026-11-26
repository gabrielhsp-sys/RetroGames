import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { obterMensagemErro } from '../../api/erros'
import { MensagemErro } from '../../components/MensagensEstado'
import { useAutenticacao } from '../../hooks/useAutenticacao'

// Só aceita destino interno ("/admin/jogos"), nunca outro site ("//exemplo.com").
function destinoSeguro(voltarPara) {
  return voltarPara && voltarPara.startsWith('/') && !voltarPara.startsWith('//') ? voltarPara : '/'
}

export default function Login() {
  const { entrar } = useAutenticacao()
  const navigate = useNavigate()
  const [parametros] = useSearchParams() // /login?voltarPara=/admin/jogos
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [mostrarSenha, setMostrarSenha] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [erro, setErro] = useState('')

  async function enviar(evento) {
    evento.preventDefault()
    setEnviando(true)
    setErro('')
    try {
      await entrar(email, senha)
      navigate(destinoSeguro(parametros.get('voltarPara')), { replace: true })
    } catch (falha) {
      setErro(obterMensagemErro(falha)) // 401, código 6: "E-mail ou senha inválidos."
      setEnviando(false)
    }
  }

  return (
    <section className="row justify-content-center align-items-center g-5 py-md-4">
      <div className="col-md-5 col-lg-4 d-none d-md-block">
        {/* Cartucho sem jogo: único adorno da tela; some no celular. */}
        <div className="cartucho cartucho-vazio cartucho-grande" aria-hidden="true">
          <div className="cartucho-casca" />
          <div className="cartucho-rotulo" />
        </div>
      </div>
      <div className="col-12 col-md-7 col-lg-5">
        <h1 className="titulo-pagina">Entrar</h1>
        <p className="subtitulo-pagina">Use o e-mail e a senha da sua conta na RetroGames.</p>

        <MensagemErro mensagem={erro} />

        <form className="painel p-4" onSubmit={enviar}>
          <div className="mb-3">
            <label htmlFor="email" className="form-label">E-mail</label>
            <input
              id="email"
              type="email"
              className="form-control"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>
          <div className="mb-4">
            <label htmlFor="senha" className="form-label">Senha</label>
            <div className="input-group">
              <input
                id="senha"
                type={mostrarSenha ? 'text' : 'password'}
                className="form-control"
                autoComplete="current-password"
                required
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
              />
              <button
                type="button"
                className="btn btn-outline-primary"
                onClick={() => setMostrarSenha(!mostrarSenha)}
                aria-pressed={mostrarSenha}
              >
                {mostrarSenha ? 'Ocultar' : 'Mostrar'}
              </button>
            </div>
          </div>
          {/* Desabilitado durante o envio: evita login duplo e mostra que algo está acontecendo. */}
          <button type="submit" className="btn btn-primary w-100" disabled={enviando}>
            {enviando ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
      </div>
    </section>
  )
}
