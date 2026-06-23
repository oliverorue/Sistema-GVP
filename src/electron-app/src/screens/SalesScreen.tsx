import { useState, useRef, useCallback, useMemo, useEffect } from 'react'
import { Search, Plus, Minus, Trash2, PauseCircle, DollarSign, X, User, Play, Clock } from 'lucide-react'
import { toast } from 'sonner'
import { useNavigate } from 'react-router-dom'
import { useCartStore } from '../stores/cartStore'
import { saleService } from '../services/saleService'
import { productService } from '../services/productService'
import { customerService } from '../services/customerService'
import { formatCurrency, formatDateTime } from '../utils/format'
import type { HeldSale, Customer } from '../types/entities'
import { Modal, ConfirmDialog } from '../components/ui'
import { SearchInput } from '../components/shared/SearchInput'
import { useKeyboardShortcuts } from '../hooks/useKeyboardShortcuts'
import { usePrintTicket } from '../hooks/usePrintTicket'
import { Logger } from '../utils/logger'

interface SearchResult {
  id: number
  name: string
  barcode: string
  price: number
  currentStock: number
  unit: string
}

export default function SalesScreen() {
  const navigate = useNavigate()
  const cart = useCartStore()
  const { printSaleTicket } = usePrintTicket()
  const searchRef = useRef<HTMLInputElement>(null)

  const [searchQuery, setSearchQuery] = useState('')
  const [searchResults, setSearchResults] = useState<SearchResult[]>([])
  const [showPayment, setShowPayment] = useState(false)
  const [loading, setLoading] = useState(false)
  const [showHeldSales, setShowHeldSales] = useState(false)
  const [heldSales, setHeldSales] = useState<HeldSale[]>([])
  const [loadingHeld, setLoadingHeld] = useState(false)
  const [showCustomerSearch, setShowCustomerSearch] = useState(false)
  const [customerQuery, setCustomerQuery] = useState('')
  const [customerResults, setCustomerResults] = useState<Customer[]>([])
  const [creditWarning, setCreditWarning] = useState<{ balance: number; limit: number } | null>(null)
  const [stayAfterSale, setStayAfterSale] = useState(true)
  const [quantityEdits, setQuantityEdits] = useState<Record<number, string>>({})
  const [cashInput, setCashInput] = useState('')
  const [discountInput, setDiscountInput] = useState('')

  const formatES = (value: string) => {
    const cleaned = value.replace(/\./g, '').replace(',', '.')
    const num = parseFloat(cleaned)
    return {
      display: isNaN(num) ? '' : num.toLocaleString('es-PY', { minimumFractionDigits: 0, maximumFractionDigits: 2 }),
      number: isNaN(num) ? 0 : num,
    }
  }

  // Init cash/discount inputs when payment modal opens
  useEffect(() => {
    if (showPayment) {
      const state = useCartStore.getState()
      setCashInput(state.cashAmount > 0 ? state.cashAmount.toLocaleString('es-PY', { minimumFractionDigits: 0, maximumFractionDigits: 2 }) : '')
      setDiscountInput(state.discount > 0 ? state.discount.toLocaleString('es-PY', { minimumFractionDigits: 0, maximumFractionDigits: 2 }) : '')
    }
  }, [showPayment])

  useEffect(() => {
    const api = window.electronAPI
    if (!api) return
    api.registerScanner()
    api.onBarcode(async (barcode: string) => {
      Logger.info('SalesScreen', 'Barcode scanned', { barcode })
      try {
        const result = await productService.search(barcode)
        if (result.isSuccess && result.data?.items?.length === 1) {
          const product = result.data.items[0]
          addToCart(product)
          toast.success(`${product.name} escaneado`)
        }
      } catch (err) {
        Logger.error('SalesScreen', 'Error al procesar codigo de barras', err)
      }
    })
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleSearch = useCallback(async (q: string) => {
    setSearchQuery(q)
    if (q.length < 2) { setSearchResults([]); return }
    try {
      const result = await productService.search(q)
      if (result.isSuccess && result.data?.items) setSearchResults(result.data.items)
    } catch (err) { Logger.error('SalesScreen', 'Error en busqueda', err) }
  }, [])

  const addToCart = useCallback((product: SearchResult) => {
    cart.addItem({
      productId: product.id,
      productName: product.name,
      barcode: product.barcode,
      quantity: 1,
      unitPrice: product.price,
      discount: 0,
      unit: product.unit,
    })
    setSearchQuery('')
    setSearchResults([])
    searchRef.current?.focus()
  }, [cart])

  const handleCheckout = async () => {
    if (cart.customerId && cart.customerName) {
      const customer = customerResults.find((c) => c.id === cart.customerId)
      if (customer && customer.creditLimit > 0 && (cart.total() + customer.balance > customer.creditLimit)) {
        setCreditWarning({ balance: customer.balance, limit: customer.creditLimit })
        return
      }
    }

    setLoading(true)
    Logger.info('SalesScreen', 'Procesando venta', { items: cart.items.length, total: cart.total() })
    try {
        const result = await saleService.create({
        customerId: cart.customerId,
        paymentMethod: cart.paymentMethod,
        cashAmount: cart.cashAmount,
        discount: cart.discount,
        notes: cart.notes,
        ivaIncluido: cart.ivaIncluido,
        taxRate: cart.taxRate,
        items: cart.items.map((i) => ({
          productId: i.productId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
          discount: i.discount,
        })),
      })

      if (result.isSuccess && result.data) {
        Logger.info('SalesScreen', 'Venta completada', { saleId: result.data.id, invoiceNumber: result.data.invoiceNumber })
        toast.success(`Venta completada — ${formatCurrency(cart.total())}`)
        const saleId = result.data.id
        cart.clearCart()
        setShowPayment(false)
        printSaleTicket(saleId)
        if (!stayAfterSale) navigate('/sales-history')
      } else {
        toast.error(result.message || 'Error al procesar la venta')
        Logger.warn('SalesScreen', 'Venta fallida', { message: result.message })
      }
    } catch (err) {
      Logger.error('SalesScreen', 'Error al procesar venta', err)
      toast.error('Error al procesar la venta')
    } finally {
      setLoading(false)
    }
  }

  const handleHold = useCallback(async () => {
    if (cart.items.length === 0) return
    try {
      const result = await saleService.holdSale({
        customerId: cart.customerId,
        paymentMethod: cart.paymentMethod,
        cashAmount: 0,
        discount: cart.discount,
        notes: cart.notes,
        ivaIncluido: cart.ivaIncluido,
        taxRate: cart.taxRate,
        items: cart.items.map((i) => ({
          productId: i.productId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
          discount: i.discount,
        })),
      })
      if (result.isSuccess) {
        toast.success('Venta pausada')
        cart.clearCart()
      } else {
        toast.error(result.message || 'Error al pausar venta')
      }
    } catch (err) {
      Logger.error('SalesScreen', 'Error al pausar venta', err)
      toast.error('Error al pausar venta')
    }
  }, [cart])

  const openHeldSales = async () => {
    setShowHeldSales(true)
    setLoadingHeld(true)
    try {
      const result = await saleService.getHeldSales()
      if (result.isSuccess && result.data) setHeldSales(result.data)
    } catch (err) { Logger.error('SalesScreen', 'Error al cargar ventas en espera', err) } finally { setLoadingHeld(false) }
  }

  const resumeHeldSale = async (id: number) => {
    try {
      const result = await saleService.resumeSale(id)
      if (result.isSuccess && result.data) {
        cart.clearCart()
        for (const item of result.data.items) {
          cart.addItem({
            productId: item.productId,
            productName: item.productName,
            barcode: item.barcode,
            quantity: item.quantity,
            unitPrice: item.unitPrice,
            discount: item.discount,
            unit: '',
          })
        }
        if (result.data.customerName) {
          cart.setCustomer(result.data.items[0]?.productId || 0, result.data.customerName)
        }
        toast.success('Venta reanudada')
        setShowHeldSales(false)
      } else toast.error(result.message)
    } catch (err) {
      Logger.error('SalesScreen', 'Error al reanudar venta', err)
      toast.error('Error al reanudar la venta')
    }
  }

  const openCustomerSearch = async () => {
    setShowCustomerSearch(true)
    setCustomerQuery('')
    setCustomerResults([])
    try {
      const result = await customerService.getAll()
      if (result.isSuccess && result.data) setCustomerResults(result.data)
    } catch (err) { Logger.error('SalesScreen', 'Error al cargar clientes', err) }
  }

  const handleCustomerSearch = useCallback(async (q: string) => {
    setCustomerQuery(q)
    if (q.length < 2) {
      try {
        const result = await customerService.getAll()
        if (result.isSuccess && result.data) setCustomerResults(result.data)
      } catch (err) { Logger.error('SalesScreen', 'Error al cargar clientes', err) }
      return
    }
    try {
      const result = await customerService.search(q)
      if (result.isSuccess && result.data) setCustomerResults(result.data)
    } catch (err) { Logger.error('SalesScreen', 'Error al buscar clientes', err) }
  }, [])

  const selectCustomer = (customer: Customer) => {
    cart.setCustomer(customer.id, customer.name)
    setShowCustomerSearch(false)
    toast.success(`Cliente: ${customer.name}`)
  }

  const continueWithCredit = () => {
    setCreditWarning(null)
    handleCheckout()
  }

  const totalTaxRate = useMemo(() => cart.taxRate * 100, [cart.taxRate])

  const shortcuts = useMemo(() => ({
    F1: () => searchRef.current?.focus(),
    F2: () => openCustomerSearch(),
    F5: () => { if (cart.items.length > 0) setShowPayment(true) },
    F8: () => handleHold(),
    Escape: () => { setShowPayment(false); setShowHeldSales(false); setShowCustomerSearch(false); setCreditWarning(null) },
  }), [cart.items.length, handleHold])

  useKeyboardShortcuts(shortcuts)

  return (
    <div className="flex gap-6 h-full">
      <div className="flex-1">
        <div className="relative mb-4">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
          <input
            ref={searchRef}
            type="text"
            value={searchQuery}
            onChange={(e) => handleSearch(e.target.value)}
            placeholder="Buscar producto por nombre o código de barras... (F1)"
            className="input-field pl-10"
            autoFocus
          />
        </div>

        {searchResults.length > 0 && (
          <div className="card mb-4 max-h-96 overflow-y-auto">
            {searchResults.map((product) => (
              <div
                key={product.id}
                className="flex items-center justify-between py-3 border-b border-slate-100 last:border-0"
              >
                <div>
                  <p className="font-medium text-slate-800">{product.name}</p>
                  <p className="text-sm text-slate-400">
                    Stock: {product.currentStock} {product.unit} — {formatCurrency(product.price)}
                  </p>
                </div>
                <button
                  onClick={() => addToCart(product)}
                  className="btn-primary text-sm py-1 px-3"
                  disabled={product.currentStock <= 0}
                >
                  <Plus className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="w-96 shrink-0">
        <div className="card h-full flex flex-col">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-slate-900">Carrito</h2>
            <div className="flex gap-1">
              <button onClick={openCustomerSearch} className="btn-secondary text-xs py-1 px-2 flex items-center gap-1" title="Seleccionar cliente (F2)">
                <User className="w-3 h-3" /> {cart.customerName || 'Cliente'}
              </button>
              <button onClick={openHeldSales} className="btn-secondary text-xs py-1 px-2 flex items-center gap-1" title="Ventas en espera">
                <Clock className="w-3 h-3" />
              </button>
            </div>
          </div>

          {cart.customerName && (
            <div className="bg-indigo-50 text-indigo-700 text-xs rounded-lg px-3 py-2 mb-3 flex items-center justify-between">
              <span>Cliente: <strong>{cart.customerName}</strong></span>
              <button onClick={() => cart.clearCustomer()} className="text-indigo-400 hover:text-indigo-600">
                <X className="w-3 h-3" />
              </button>
            </div>
          )}

          <div className="flex-1 overflow-y-auto space-y-2 mb-4">
            {cart.items.map((item) => (
              <div key={item.productId} className="bg-slate-50 rounded-lg p-3">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <p className="text-sm font-medium text-slate-800">{item.productName}</p>
                    <p className="text-xs text-slate-400">{formatCurrency(item.unitPrice)} c/u</p>
                  </div>
                  <button onClick={() => cart.removeItem(item.productId)} className="text-red-400 hover:text-red-600">
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
                <div className="flex items-center justify-between mt-2">
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => cart.updateQuantity(item.productId, +(item.quantity - 1).toFixed(2))}
                      className="w-6 h-6 bg-white border rounded flex items-center justify-center hover:bg-slate-100"
                    >
                      <Minus className="w-3 h-3" />
                    </button>
                    <input
                      type="text"
                      inputMode="decimal"
                      value={quantityEdits[item.productId] ?? String(item.quantity)}
                      onChange={(e) => {
                        setQuantityEdits((prev) => ({ ...prev, [item.productId]: e.target.value }))
                      }}
                      onBlur={() => {
                        const raw = quantityEdits[item.productId]
                        if (raw !== undefined) {
                          const val = parseFloat(raw.replace(',', '.'))
                          if (!isNaN(val) && val > 0 && val !== item.quantity) {
                            cart.updateQuantity(item.productId, val)
                          }
                          setQuantityEdits((prev) => {
                            const copy = { ...prev }
                            delete copy[item.productId]
                            return copy
                          })
                        }
                      }}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') (e.target as HTMLInputElement).blur()
                      }}
                      className="w-16 h-7 text-center text-sm font-medium border border-slate-200 rounded focus:outline-none focus:border-indigo-400 focus:ring-1 focus:ring-indigo-300"
                      placeholder={String(item.quantity)}
                    />
                    <button
                      onClick={() => cart.updateQuantity(item.productId, +(item.quantity + 1).toFixed(2))}
                      className="w-6 h-6 bg-white border rounded flex items-center justify-center hover:bg-slate-100"
                    >
                      <Plus className="w-3 h-3" />
                    </button>
                  </div>
                  <div className="text-right">
                    <span className="text-sm font-semibold block">{formatCurrency(item.subtotal)}</span>
                    {item.unit && <span className="text-xs text-slate-400">{item.unit}</span>}
                  </div>
                </div>
              </div>
            ))}

            {cart.items.length === 0 && (
              <p className="text-center text-slate-400 py-8">Carrito vacío</p>
            )}
          </div>

          <div className="border-t border-slate-200 pt-4 space-y-2">
            <div className="flex justify-between text-sm text-slate-600">
              <span>Subtotal</span>
              <span>{formatCurrency(cart.subtotal())}</span>
            </div>
            <div className="flex justify-between text-sm text-slate-600">
              <span>IVA ({totalTaxRate.toFixed(0)}%)</span>
              <span>{formatCurrency(cart.tax())}</span>
            </div>
            {cart.discount > 0 && (
              <div className="flex justify-between text-sm text-red-500">
                <span>Descuento</span>
                <span>-{formatCurrency(cart.discount)}</span>
              </div>
            )}
            <div className="flex justify-between text-lg font-bold text-slate-900 pt-2 border-t">
              <span>Total</span>
              <span>{formatCurrency(cart.total())}</span>
            </div>
          </div>

          <label className="flex items-center gap-2 text-xs text-slate-500 cursor-pointer mt-2">
            <input type="checkbox" checked={stayAfterSale} onChange={(e) => setStayAfterSale(e.target.checked)} className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500" />
            Modo venta continua (quedarse después de cobrar)
          </label>

          <div className="flex gap-2 mt-2">
            <button
              onClick={handleHold}
              disabled={cart.items.length === 0}
              className="btn-secondary flex-1 flex items-center justify-center gap-2"
            >
              <PauseCircle className="w-4 h-4" /> Pausar
            </button>
            <button
              onClick={() => setShowPayment(true)}
              disabled={cart.items.length === 0}
              className="btn-primary flex-1 flex items-center justify-center gap-2"
            >
              <DollarSign className="w-4 h-4" /> Cobrar
            </button>
          </div>
        </div>
      </div>

      {showPayment && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold">Cobrar Venta</h3>
              <button onClick={() => setShowPayment(false)}><X className="w-5 h-5" /></button>
            </div>

            <p className="text-3xl font-bold text-center text-indigo-600 mb-6">
              {formatCurrency(cart.total())}
            </p>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Método de pago</label>
                <select
                  value={cart.paymentMethod}
                  onChange={(e) => cart.setPaymentMethod(e.target.value as 'Cash' | 'Card' | 'Transfer' | 'Credit')}
                  className="input-field"
                >
                  <option value="Cash">Efectivo</option>
                  <option value="Card">Tarjeta</option>
                  <option value="Transfer">Transferencia</option>
                  <option value="Credit">Crédito</option>
                </select>
              </div>

              <div>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={cart.ivaIncluido}
                    onChange={(e) => cart.setIvaIncluido(e.target.checked)}
                    className="rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                  />
                  <span className="text-sm text-slate-700">
                    IVA incluido ({totalTaxRate.toFixed(0)}%)
                  </span>
                </label>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Descuento global</label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm">Gs.</span>
                  <input
                    type="text"
                    inputMode="decimal"
                    value={discountInput}
                    onChange={(e) => {
                      const { display, number } = formatES(e.target.value)
                      setDiscountInput(display)
                      cart.setGlobalDiscount(number)
                    }}
                    className="input-field pl-10"
                    placeholder="0"
                  />
                </div>
              </div>

              {cart.paymentMethod === 'Cash' && (
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">Efectivo recibido</label>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm font-medium">Gs.</span>
                    <input
                      type="text"
                      inputMode="decimal"
                      value={cashInput}
                      onChange={(e) => {
                        const { display, number } = formatES(e.target.value)
                        setCashInput(display)
                        cart.setCashAmount(number)
                      }}
                      className="input-field pl-10 text-lg font-semibold"
                      placeholder="0"
                    />
                  </div>
                  {cart.cashAmount >= cart.total() && cart.cashAmount > 0 && (
                    <p className="text-sm text-emerald-600 mt-1 flex items-center gap-1">
                      <span className="text-lg">↩</span> Cambio: {formatCurrency(cart.cashAmount - cart.total())}
                    </p>
                  )}
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Notas</label>
                <input
                  type="text"
                  value={cart.notes}
                  onChange={(e) => cart.setNotes(e.target.value)}
                  className="input-field"
                  placeholder="Opcional"
                />
              </div>

              <div className="flex gap-2">
                <button onClick={() => setShowPayment(false)} className="btn-secondary flex-1">Cancelar</button>
                <button onClick={handleCheckout} disabled={loading} className="btn-success flex-1">
                  {loading ? 'Procesando...' : 'Cobrar'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <Modal isOpen={showCustomerSearch} onClose={() => setShowCustomerSearch(false)} title="Seleccionar Cliente" size="md">
        <div className="space-y-4">
          <SearchInput value={customerQuery} onChange={handleCustomerSearch} placeholder="Buscar cliente..." />
          <div className="max-h-72 overflow-y-auto space-y-1">
            {customerResults.map((c) => (
              <button
                key={c.id}
                onClick={() => selectCustomer(c)}
                className="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-50 border border-transparent hover:border-slate-200 transition-all"
              >
                <p className="font-medium text-sm">{c.name}</p>
                <p className="text-xs text-slate-400">
                  {c.taxId && `${c.taxId} — `}Saldo: {formatCurrency(c.balance)}{c.creditLimit > 0 ? ` / Límite: ${formatCurrency(c.creditLimit)}` : ''}
                </p>
              </button>
            ))}
            {customerResults.length === 0 && (
              <p className="text-sm text-slate-400 text-center py-4">Sin resultados</p>
            )}
          </div>
        </div>
      </Modal>

      <Modal isOpen={showHeldSales} onClose={() => setShowHeldSales(false)} title="Ventas en Espera" size="md">
        {loadingHeld ? (
          <p className="text-center text-slate-400 py-4">Cargando...</p>
        ) : heldSales.length === 0 ? (
          <p className="text-center text-slate-400 py-4">No hay ventas en espera</p>
        ) : (
          <div className="space-y-2 max-h-80 overflow-y-auto">
            {heldSales.map((sale) => (
              <div key={sale.id} className="flex items-center justify-between p-3 rounded-lg border border-slate-200">
                <div>
                  <p className="text-sm font-medium">{sale.customerName || 'Sin cliente'}</p>
                  <p className="text-xs text-slate-400">
                    {formatDateTime(sale.heldAt)} — {sale.items?.length || 0} items
                  </p>
                  <p className="text-sm font-semibold text-indigo-600 mt-1">{formatCurrency(sale.saleTotal)}</p>
                </div>
                <button onClick={() => resumeHeldSale(sale.id)} className="btn-primary text-sm py-1.5 px-3 flex items-center gap-1">
                  <Play className="w-3 h-3" /> Reanudar
                </button>
              </div>
            ))}
          </div>
        )}
      </Modal>

      <ConfirmDialog
        isOpen={creditWarning !== null}
        onClose={() => setCreditWarning(null)}
        onConfirm={continueWithCredit}
        title="Advertencia de Crédito"
        message={
          creditWarning
            ? `El cliente superará su límite de crédito. Saldo actual: ${formatCurrency(creditWarning.balance)}. Límite: ${formatCurrency(creditWarning.limit)}. ¿Continuar?`
            : ''
        }
        confirmLabel="Continuar"
        variant="warning"
      />
    </div>
  )
}
