import { useCallback } from 'react'
import { toast } from 'sonner'
import { settingsService } from '../services/settingsService'
import { saleService } from '../services/saleService'
import { renderTicketHTML } from '../components/shared/TicketTemplate'
import { Logger } from '../utils/logger'
import type { Company } from '../types/entities'

export function usePrintTicket() {
  const printSaleTicket = useCallback(async (saleId: number) => {
    try {
      const [saleResult, companyResult] = await Promise.all([
        saleService.getById(saleId),
        settingsService.getCompany(),
      ])

      if (!saleResult.isSuccess || !saleResult.data) {
        toast.error('Error al obtener datos de la venta')
        return
      }

      const sale = saleResult.data
      const company: Company = companyResult.isSuccess && companyResult.data
        ? companyResult.data
        : { id: 0, name: 'Mi Empresa', taxId: '', address: '', phone: '', email: '', logoUrl: '', taxRate: 0.10, ivaIncluido: true, currency: 'Gs.', lowStockThreshold: 10, isActive: true, createdAt: '' }

      const html = renderTicketHTML(sale, company)

      if (window.electronAPI?.printTicket) {
        const result = await window.electronAPI.printTicket(html)
        if (result.success) {
          toast.success('Ticket impreso')
        } else {
          Logger.error('usePrintTicket', 'Fallo de impresion', result.message)
          toast.error(result.message || 'Error al imprimir ticket')
        }
      } else {
        // Browser fallback: open HTML in a new window for printing
        const w = window.open('', '_blank')
        if (w) {
          w.document.open()
          w.document.write(html)
          w.document.close()
          w.onafterprint = () => w.close()
          w.focus()
          w.print()
        }
      }
    } catch (err) {
      Logger.error('usePrintTicket', 'Error al imprimir ticket', err)
      toast.error('Error al imprimir el ticket')
    }
  }, [])

  return { printSaleTicket }
}
