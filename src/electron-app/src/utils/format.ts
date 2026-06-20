export function formatCurrency(amount: number, currency = 'Gs.'): string {
  return `${currency} ${amount.toLocaleString('es-PY', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`
}

export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date
  return d.toLocaleDateString('es-PY', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

export function formatDateTime(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date
  return d.toLocaleDateString('es-PY', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatTime(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date
  return d.toLocaleTimeString('es-PY', { hour: '2-digit', minute: '2-digit' })
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1048576).toFixed(1)} MB`
}
