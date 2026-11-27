import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { obterErrosDosCampos, obterMensagemErro } from '../../api/erros'
import { MensagemErro } from '../../components/MensagensEstado'
import { useAutenticacao } from '../../hooks/useAutenticacao'
import { comVoltarPara, destinoSeguro } from '../../lib/destino'

const EMAIL_JA_CADASTRADO = 106

// C01: cadastro de cliente. A API cria a conta (perfil Cliente, senha em hash)
// e já inicia a sessão; o AutenticacaoProvider guarda o usuário e o menu muda sem F5.
export default function Cadastro() {
  const { cadastrar } = useAutenticacao()
  const navigate = useNavigate()
  const [parametros] = useSearchParams() // /cadastro?voltarPara=/carrinho
  const voltarPara = parametros.get('voltarPara')
  const [formulario, setFormulario] = useState({ nome: '', email: '', senha: '' })
  const [mostrarSenha, setMostrarSenha] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [errosCampos, setErrosCampos] = useState({})
  const [erro, setErro] = useState('')

  function alterarCampo(evento) {
    const { name, value } = evento.target
    setFormulario({ ...formulario, [name]: value })
  }

  async function enviar(evento) {
    evento.preventDefault()
    setEnviando(true)
    setErrosCampos({})
    setErro('')
    try {
      const usuario = await cadastrar(formulario.nome.trim(), formulario.email.trim(), formulario.senha)
      navigate(destinoSeguro(voltarPara), {
        replace: true,
        state: { avisoGlobal: `Conta criada. Boas-vindas, ${usuario.nome}!` },
      })
    } catch (falha) {
      // 409, código 106: o problema é o e-mail, então a mensagem vai embaixo dele.
      const ehEmailRepetido = falha?.response?.data?.responseCode === EMAIL_JA_CADASTRADO
      const campos = ehEmailRepetido ? { email: obterMensagemErro(falha) } : obterErrosDosCampos(falha)
      setErrosCampos(campos)
      // 400 (código 3) já aparece em cada campo; o resto (rede, servidor) vai no topo.
      if (Object.keys(campos).length === 0) setErro(obterMensagemErro(falha))
      setEnviando(false)
      // Leva o foco ao primeiro campo com erro, para o teclado e o leitor de tela.
      const primeiro = ['nome', 'email', 'senha'].find((campo) => campos[campo])
      if (primeiro) window.requestAnimationFrame(() => document.getElementById(primeiro)?.focus())
    }
  }

  const classe = (campo) => `form-control ${errosCampos[campo] ? 'is-invalid' : ''}`
  const descricao = (campo, ajuda) =>
    [errosCampos[campo] ? `erro-${campo}` : null, ajuda].filter(Boolean).join(' ') || undefined
  const mensagemDoCampo = (campo) => (
    <div id={`erro-${campo}`} className="invalid-feedback">{errosCampos[campo]}</div>
  )

  return (
    <section className="row justify-content-center align-items-center g-4 g-lg-5 py-md-4">
      <div className="col-md-5 col-lg-4 d-none d-md-block">
        {/* Mesmo adorno do Login: as duas telas são a mesma porta de entrada. */}
        <div className="cartucho cartucho-vazio cartucho-grande" aria-hidden="true">
          <div className="cartucho-casca" />
          <div className="cartucho-rotulo" />
        </div>
      </div>
      <div className="col-12 col-md-7 col-lg-5">
        <h1 className="titulo-pagina">Criar conta</h1>
        <p className="subtitulo-pagina">Com a conta, você finaliza compras e acompanha os seus pedidos.</p>

        <MensagemErro mensagem={erro} />

        <form className="painel p-4" onSubmit={enviar} noValidate>
          <div className="mb-3">
            <label htmlFor="nome" className="form-label">Nome</label>
            <input
              id="nome" name="nome" className={classe('nome')} autoComplete="name" maxLength={100}
              required value={formulario.nome} onChange={alterarCampo}
              aria-invalid={Boolean(errosCampos.nome)} aria-describedby={descricao('nome')}
            />
            {mensagemDoCampo('nome')}
          </div>
          <div className="mb-3">
            <label htmlFor="email" className="form-label">E-mail</label>
            <input
              id="email" name="email" type="email" className={classe('email')} autoComplete="email"
              maxLength={256} required value={formulario.email} onChange={alterarCampo}
              aria-invalid={Boolean(errosCampos.email)} aria-describedby={descricao('email')}
            />
            {mensagemDoCampo('email')}
          </div>
          <div className="mb-4">
            <label htmlFor="senha" className="form-label">Senha</label>
            {/* has-validation: o input-group do Bootstrap mostra o erro com os cantos certos. */}
            <div className="input-group has-validation">
              <input
                id="senha" name="senha" type={mostrarSenha ? 'text' : 'password'} className={classe('senha')}
                autoComplete="new-password" required minLength={8} value={formulario.senha} onChange={alterarCampo}
                aria-invalid={Boolean(errosCampos.senha)} aria-describedby={descricao('senha', 'ajuda-senha')}
              />
              <button
                type="button" className="btn btn-outline-primary"
                onClick={() => setMostrarSenha(!mostrarSenha)} aria-pressed={mostrarSenha}
              >
                {mostrarSenha ? 'Ocultar' : 'Mostrar'}
              </button>
              {mensagemDoCampo('senha')}
            </div>
            {/* A regra fica à vista antes do erro (heurística 10: ajuda quando ela é necessária). */}
            <div id="ajuda-senha" className="form-text">Mínimo de 8 caracteres.</div>
          </div>
          <button type="submit" className="btn btn-primary w-100" disabled={enviando}>
            {enviando ? 'Criando conta...' : 'Criar conta'}
          </button>
        </form>

        <p className="mt-3 mb-0 text-center">
          Já tem conta?{' '}
          <Link to={comVoltarPara('/login', voltarPara)} className="fw-semibold link-toque">Já tenho conta</Link>
        </p>
      </div>
    </section>
  )
}
