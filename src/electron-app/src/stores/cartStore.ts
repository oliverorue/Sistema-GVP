import { create } from 'zustand'

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

interface CartState {
  items: CartItem[]
  customerId?: number
  customerName?: string
  paymentMethod: 'Cash' | 'Card' | 'Transfer'
  cashAmount: number
  notes: string
  discount: number
  taxRate: number
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
  clearCart: () => void
  subtotal: () => number
  tax: () => number
  total: () => number
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],
  paymentMethod: 'Cash',
  cashAmount: 0,
  notes: '',
  discount: 0,
  taxRate: 0.10,

  addItem: (item) => {
    const items = get().items
    const existing = items.find((i) => i.productId === item.productId)
    if (existing) {
      const newQty = existing.quantity + item.quantity
      set({
        items: items.map((i) =>
          i.productId === item.productId
            ? { ...i, quantity: newQty, subtotal: newQty * i.unitPrice - i.discount }
            : i
        ),
      })
    } else {
      set({
        items: [
          ...items,
          { ...item, subtotal: item.quantity * item.unitPrice - item.discount },
        ],
      })
    }
  },

  removeItem: (productId) => {
    set({ items: get().items.filter((i) => i.productId !== productId) })
  },

  updateQuantity: (productId, quantity) => {
    set({
      items: get().items.map((i) =>
        i.productId === productId
          ? { ...i, quantity, subtotal: quantity * i.unitPrice - i.discount }
          : i
      ),
    })
  },

  updateDiscount: (productId, discount) => {
    set({
      items: get().items.map((i) =>
        i.productId === productId
          ? { ...i, discount, subtotal: i.quantity * i.unitPrice - discount }
          : i
      ),
    })
  },

  setCustomer: (id, name) => set({ customerId: id, customerName: name }),
  clearCustomer: () => set({ customerId: undefined, customerName: undefined }),
  setPaymentMethod: (method) => set({ paymentMethod: method }),
  setCashAmount: (amount) => set({ cashAmount: amount }),
  setNotes: (notes) => set({ notes }),
  setGlobalDiscount: (discount) => set({ discount }),
  setTaxRate: (rate) => set({ taxRate: rate }),

  clearCart: () =>
    set({
      items: [],
      customerId: undefined,
      customerName: undefined,
      paymentMethod: 'Cash',
      cashAmount: 0,
      notes: '',
      discount: 0,
    }),

  subtotal: () => get().items.reduce((sum, i) => sum + i.subtotal, 0),
  tax: () => get().subtotal() * get().taxRate,
  total: () => get().subtotal() + get().tax() - get().discount,
}))
