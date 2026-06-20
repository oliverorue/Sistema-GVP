import { useCallback } from 'react'
import { toast } from 'sonner'
import { settingsService } from '../services/settingsService'
import { saleService } from '../services/saleService'
import { renderTicketHTML } from '../components/shared/TicketTemplate'
import type { Sale } from '../types/entities'

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
      const company = companyResult.isSuccess && companyResult.data
        ? companyResult.data
        : { name: 'Mi Empresa', taxId: '', address: '', phone: '', email: '', taxRate: 0.10, currency: 'Gs.', lowStockThreshold: 10, isActive: true, createdAt: '' }

      const html = renderTicketHTML(sale, company as any)

      if (window.electronAPI?.printTicket) {
        await window.electronAPI.printTicket(html)
        toast.success('Imprimiendo ticket...')
      } else {
        const w = window.open('', '_blank')
        if (w) {
          w.document.write(html)
          w.document.close()
          w.print()
        }
      }
    } catch {
      toast.error('Error al imprimir el ticket')
    }
  }, [])

  return { printSaleTicket }
}
