import type { Sale, Company } from '../../types/entities'
import { formatCurrency } from '../../utils/format'

export function renderTicketHTML(sale: Sale, company: Company): string {
  const baseAmount = company.ivaIncluido
    ? sale.subtotal / (1 + company.taxRate)
    : sale.subtotal

  const itemsHtml = sale.items
    .map(
      (item) => `
    <tr>
      <td class="qty">${item.quantity} ${item.unit || ''} x ${item.productName}</td>
      <td class="amount">${formatCurrency(item.subtotal)}</td>
    </tr>`
    )
    .join('')

  return `<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Ticket</title>
<style>
  @page { margin: 0; size: 80mm auto; }
  body {
    font-family: 'Courier New', monospace;
    font-size: 12px;
    width: 72mm;
    margin: 0 auto;
    padding: 5px 2mm;
    color: #000;
  }
  .header { text-align: center; margin-bottom: 8px; }
  .header h1 { font-size: 16px; margin: 0; font-weight: bold; }
  .header p { margin: 2px 0; font-size: 10px; }
  .line { border-top: 1px dashed #000; margin: 6px 0; }
  .info-table { width: 100%; font-size: 10px; }
  .info-table td { padding: 1px 0; }
  table.items { width: 100%; border-collapse: collapse; font-size: 11px; }
  table.items td { padding: 2px 0; }
  table.items td.qty { }
  table.items td.amount { text-align: right; white-space: nowrap; }
  .totals { width: 100%; font-size: 11px; }
  .totals td { padding: 2px 0; }
  .totals td.label { }
  .totals td.value { text-align: right; white-space: nowrap; }
  .totals .grand-total { font-size: 14px; font-weight: bold; }
  .footer { text-align: center; margin-top: 8px; font-size: 10px; }
  .payment-info { font-size: 10px; margin-top: 4px; }
</style>
</head>
<body>
  <div class="header">
    <h1>${company.name}</h1>
    <p>RUC: ${company.taxId}</p>
    ${company.address ? `<p>${company.address}</p>` : ''}
    ${company.phone ? `<p>Tel: ${company.phone}</p>` : ''}
  </div>

  <div class="line"></div>

  <table class="info-table">
    <tr><td>Factura: <strong>${sale.invoiceNumber}</strong></td></tr>
    <tr><td>Fecha: ${new Date(sale.createdAt).toLocaleString('es-PY')}</td></tr>
    <tr><td>Cajero: ${sale.userName}</td></tr>
    ${sale.customerName ? `<tr><td>Cliente: ${sale.customerName}</td></tr>` : ''}
  </table>

  <div class="line"></div>

  <table class="items">
    <thead>
      <tr style="font-weight:bold"><td>Producto</td><td class="amount">Subtotal</td></tr>
    </thead>
    <tbody>
      ${itemsHtml}
    </tbody>
  </table>

  <div class="line"></div>

  <table class="totals">
    ${company.ivaIncluido ? `<tr><td class="label">Base imponible</td><td class="value">${formatCurrency(baseAmount)}</td></tr>` : ''}
    <tr><td class="label">IVA ${(company.taxRate * 100).toFixed(0)}%${company.ivaIncluido ? ' (incluido)' : ''}</td><td class="value">${formatCurrency(sale.tax)}</td></tr>
    ${sale.discount > 0 ? `<tr><td class="label">Descuento</td><td class="value">-${formatCurrency(sale.discount)}</td></tr>` : ''}
    <tr class="grand-total"><td class="label"><strong>TOTAL</strong></td><td class="value"><strong>${formatCurrency(sale.total)}</strong></td></tr>
  </table>

  <div class="payment-info">
    <p>Método de pago: ${sale.paymentMethod === 'Cash' ? 'Efectivo' : sale.paymentMethod === 'Card' ? 'Tarjeta' : sale.paymentMethod === 'Transfer' ? 'Transferencia' : 'Crédito'}</p>
    ${sale.paymentMethod === 'Cash' && sale.cashAmount > 0 ? `<p>Efectivo recibido: ${formatCurrency(sale.cashAmount)}</p>` : ''}
    ${sale.paymentMethod === 'Cash' && sale.changeAmount > 0 ? `<p>Cambio: ${formatCurrency(sale.changeAmount)}</p>` : ''}
  </div>

  <div class="line"></div>

  <div class="footer">
    <p>Gracias por su compra</p>
    <p>Sistema GVP v2.0</p>
  </div>
</body>
</html>`
}
