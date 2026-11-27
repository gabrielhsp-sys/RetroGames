const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

// 199.9 → "R$ 199,90"
export function formatarPreco(valor) {
  return moeda.format(valor)
}

const dataHora = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

// A API grava a data em UTC e a devolve sem fuso ("2026-11-27T14:05:00").
// Sem fuso, o texto é tratado como UTC e mostrado no fuso de quem está vendo:
// "2026-11-27T14:05:00" → "27/11/2026 11:05" em Brasília.
export function formatarDataHora(valor) {
  const temFuso = /(Z|[+-]\d{2}:?\d{2})$/.test(valor)
  const data = new Date(temFuso ? valor : `${valor}Z`)
  if (Number.isNaN(data.getTime())) return ''
  const partes = Object.fromEntries(dataHora.formatToParts(data).map((p) => [p.type, p.value]))
  return `${partes.day}/${partes.month}/${partes.year} ${partes.hour}:${partes.minute}`
}
