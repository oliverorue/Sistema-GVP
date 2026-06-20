import { useEffect } from 'react'

type ShortcutMap = Record<string, () => void>

export function useKeyboardShortcuts(shortcuts: ShortcutMap) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const key = e.key === 'Escape' ? 'Escape' : `F${e.key.match(/^F(\d+)$/)?.[1] || ''}`
      const action = shortcuts[key]
      if (action) {
        e.preventDefault()
        action()
      }
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [shortcuts])
}
