import { Modal } from './Modal'

interface ConfirmDialogProps {
  isOpen: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string
  confirmLabel?: string
  variant?: 'danger' | 'warning'
}

const variantButtonClasses = {
  danger: 'btn-danger',
  warning: 'btn-warning bg-amber-500 text-white px-4 py-2 rounded-lg font-medium hover:bg-amber-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed',
}

export function ConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel = 'Confirmar',
  variant = 'danger',
}: ConfirmDialogProps) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="sm">
      <p className="text-sm text-slate-600 mb-6">{message}</p>
      <div className="flex justify-end gap-3">
        <button onClick={onClose} className="btn-secondary">
          Cancelar
        </button>
        <button onClick={onConfirm} className={variantButtonClasses[variant]}>
          {confirmLabel}
        </button>
      </div>
    </Modal>
  )
}
