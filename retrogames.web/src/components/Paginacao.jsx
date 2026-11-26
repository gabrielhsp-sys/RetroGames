// Anterior, números e Próxima. Não aparece quando há uma página só.
export default function Paginacao({ pagina, totalPaginas, aoMudar }) {
  if (totalPaginas <= 1) return null
  const paginas = Array.from({ length: totalPaginas }, (_, i) => i + 1)

  return (
    <nav aria-label="Páginas do resultado" className="mt-4">
      <ul className="pagination justify-content-center flex-wrap">
        <li className={`page-item ${pagina === 1 ? 'disabled' : ''}`}>
          <button type="button" className="page-link" onClick={() => aoMudar(pagina - 1)} disabled={pagina === 1}>
            Anterior
          </button>
        </li>
        {paginas.map((numero) => (
          <li key={numero} className={`page-item ${numero === pagina ? 'active' : ''}`}>
            <button
              type="button"
              className="page-link"
              onClick={() => aoMudar(numero)}
              aria-current={numero === pagina ? 'page' : undefined}
              aria-label={`Página ${numero}`}
            >
              {numero}
            </button>
          </li>
        ))}
        <li className={`page-item ${pagina === totalPaginas ? 'disabled' : ''}`}>
          <button type="button" className="page-link" onClick={() => aoMudar(pagina + 1)} disabled={pagina === totalPaginas}>
            Próxima
          </button>
        </li>
      </ul>
    </nav>
  )
}
