// Os três estados de toda tela que busca dados: carregando, erro e vazio.
// Fazem parte do visual: mesmo painel claro, cada um com o seu sinal.

export function Carregando({ texto = 'Carregando...' }) {
  return (
    <div role="status" className="estado">
      <span className="carregando-sulcos" aria-hidden="true">
        <span /><span /><span /><span />
      </span>
      <span className="fw-semibold">{texto}</span>
    </div>
  )
}

export function MensagemErro({ mensagem, aoTentarNovamente }) {
  if (!mensagem) return null
  return (
    <div role="alert" className="estado estado-erro flex-wrap">
      <span className="estado-icone" aria-hidden="true">!</span>
      <span className="me-auto fw-semibold">{mensagem}</span>
      {aoTentarNovamente && (
        <button type="button" className="btn btn-outline-danger" onClick={aoTentarNovamente}>
          Tentar novamente
        </button>
      )}
    </div>
  )
}

export function EstadoVazio({ titulo, children }) {
  return (
    <div className="estado-vazio">
      {/* Cartucho sem jogo: o desenho do "não tem nada aqui". */}
      <div className="cartucho cartucho-vazio" aria-hidden="true">
        <div className="cartucho-casca" />
        <div className="cartucho-rotulo" />
      </div>
      <h2 className="h4">{titulo}</h2>
      {children}
    </div>
  )
}
