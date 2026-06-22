import { create } from 'zustand'
import { Logger } from '../utils/logger'

const STORAGE_KEY = 'gvp_cart_state'

interface CartItem {
  productId: number
  productName: string
  barcode: string
  quantity: number
  unitPrice: number
  discount: number
  subtotal: number
  unit: string
}

interface PersistedState {
  items: CartItem[]
  customerId?: number
  customerName?: string
  paymentMethod: 'Cash' | 'Card' | 'Transfer'
  discount: number
  ivaIncluido: boolean
}

interface CartState {
  items: CartItem[]
  customerId?: number
  customerName?: string
  paymentMethod: 'Cash' | 'Card' | 'Transfer'
  cashAmount: number
  notes: string
  discount: number
  taxRate: number
  ivaIncluido: boolean
  addItem: (item: Omit<CartItem, 'subtotal'>) => void
  removeItem: (productId: number) => void
  updateQuantity: (productId: number, quantity: number) => void
  updateDiscount: (productId: number, discount: number) => void
  setCustomer: (id: number, name: string) => void
  clearCustomer: () => void
  setPaymentMethod: (method: 'Cash' | 'Card' | 'Transfer') => void
  setCashAmount: (amount: number) => void
  setNotes: (notes: string) => void
  setGlobalDiscount: (discount: number) => void
  setTaxRate: (rate: number) => void
  setIvaIncluido: (ivaIncluido: boolean) => void
  clearCart: () => void
  subtotal: () => number
  tax: () => number
  total: () => number
}

function loadPersistedState(): Partial<PersistedState> {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved) {
      const parsed = JSON.parse(saved) as PersistedState
      Logger.info('cartStore', 'Carrito cargado de localStorage', { items: parsed.items?.length })
      return parsed
    }
  } catch (e) {
    Logger.error('cartStore', 'Error al cargar carrito', e)
  }
  return {}
}

function savePersistedState(state: CartState): void {
  try {
    const toSave: PersistedState = {
      items: state.items,
      customerId: state.customerId,
      customerName: state.customerName,
      paymentMethod: state.paymentMethod,
      discount: state.discount,
      ivaIncluido: state.ivaIncluido,
    }
    localStorage.setItem(STORAGE_KEY, JSON.stringify(toSave))
  } catch (e) {
    Logger.error('cartStore', 'Error al guardar carrito', e)
  }
}

const persisted = loadPersistedState()

export const useCartStore = create<CartState>((set, get) => ({
  items: persisted.items ?? [],
  customerId: persisted.customerId,
  customerName: persisted.customerName,
  paymentMethod: persisted.paymentMethod ?? 'Cash',
  cashAmount: 0,
  notes: '',
  discount: persisted.discount ?? 0,
  taxRate: 0.10,
  ivaIncluido: persisted.ivaIncluido ?? true,

  addItem: (item) => {
    const items = get().items
    const existing = items.find((i) => i.productId === item.productId)
    let newItems: CartItem[]
    if (existing) {
      const newQty = existing.quantity + item.quantity
      newItems = items.map((i) =>
        i.productId === item.productId
          ? { ...i, quantity: newQty, subtotal: newQty * i.unitPrice - i.discount }
          : i
      )
    } else {
      newItems = [...items, { ...item, subtotal: item.quantity * item.unitPrice - item.discount }]
    }
    set({ items: newItems })
    savePersistedState({ ...get(), items: newItems })
  },

  removeItem: (productId) => {
    const newItems = get().items.filter((i) => i.productId !== productId)
    set({ items: newItems })
    savePersistedState({ ...get(), items: newItems })
  },

  updateQuantity: (productId, quantity) => {
    if (quantity <= 0) { get().removeItem(productId); return }
    const newItems = get().items.map((i) =>
      i.productId === productId
        ? { ...i, quantity, subtotal: quantity * i.unitPrice - i.discount }
        : i
    )
    set({ items: newItems })
    savePersistedState({ ...get(), items: newItems })
  },

  updateDiscount: (productId, discount) => {
    const newItems = get().items.map((i) =>
      i.productId === productId
        ? { ...i, discount, subtotal: i.quantity * i.unitPrice - discount }
        : i
    )
    set({ items: newItems })
    savePersistedState({ ...get(), items: newItems })
  },

  setCustomer: (id, name) => {
    set({ customerId: id, customerName: name })
    savePersistedState({ ...get(), customerId: id, customerName: name })
  },

  clearCustomer: () => {
    set({ customerId: undefined, customerName: undefined })
    savePersistedState({ ...get(), customerId: undefined, customerName: undefined })
  },

  setPaymentMethod: (method) => {
    set({ paymentMethod: method })
    savePersistedState({ ...get(), paymentMethod: method })
  },

  setCashAmount: (amount) => set({ cashAmount: amount }),
  setNotes: (notes) => set({ notes }),
  setGlobalDiscount: (discount) => {
    set({ discount })
    savePersistedState({ ...get(), discount })
  },
  setTaxRate: (rate) => set({ taxRate: rate }),
  setIvaIncluido: (ivaIncluido) => {
    set({ ivaIncluido })
    savePersistedState({ ...get(), ivaIncluido })
  },

  clearCart: () => {
    localStorage.removeItem(STORAGE_KEY)
    set({
      items: [],
      customerId: undefined,
      customerName: undefined,
      paymentMethod: 'Cash',
      cashAmount: 0,
      notes: '',
      discount: 0,
    })
  },

  subtotal: () => get().items.reduce((sum, i) => sum + i.subtotal, 0),
  tax: () => {
    const s = get()
    if (s.ivaIncluido) {
      const base = (s.subtotal() - s.discount) / (1 + s.taxRate)
      return (s.subtotal() - s.discount) - base
    }
    return (s.subtotal() - s.discount) * s.taxRate
  },
  total: () => {
    const s = get()
    if (s.ivaIncluido) return s.subtotal() - s.discount
    return s.subtotal() + s.tax() - s.discount
  },
}))
