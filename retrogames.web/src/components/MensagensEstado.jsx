// Os três estados de toda tela que busca dados: carregando, erro e vazio.

export function Carregando({ texto = 'Carregando...' }) {
  return (
    <div role="status" className="d-flex align-items-center gap-2 py-4">
      <span className="spinner-border spinner-border-sm" aria-hidden="true" />
      <span>{texto}</span>
    </div>
  )
}

export function MensagemErro({ mensagem, aoTentarNovamente }) {
  if (!mensagem) return null
  return (
    <div role="alert" className="alert alert-danger d-flex flex-wrap align-items-center gap-2">
      <span className="me-auto">{mensagem}</span>
      {aoTentarNovamente && (
        <button type="button" className="btn btn-outline-danger btn-sm" onClick={aoTentarNovamente}>
          Tentar novamente
        </button>
      )}
    </div>
  )
}

export function EstadoVazio({ titulo, children }) {
  return (
    <div className="text-center py-5">
      <h2 className="h5">{titulo}</h2>
      {children}
    </div>
  )
}
