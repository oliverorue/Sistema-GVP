import { useState, useEffect } from 'react'
import { Save } from 'lucide-react'
import { settingsService } from '../services/settingsService'
import { Company } from '../types/entities'

export default function SettingsScreen() {
  const [company, setCompany] = useState<Company | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    const fetch = async () => {
      try {
        const result = await settingsService.getCompany()
        if (result.isSuccess) setCompany(result.data)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }
    fetch()
  }, [])

  const handleSave = async () => {
    if (!company) return
    setSaving(true)
    try {
      await settingsService.updateCompany(company)
    } catch (err) {
      console.error(err)
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <div className="flex items-center justify-center h-64"><div className="w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin" /></div>

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-slate-900">Configuracion</h1>
      <div className="card max-w-2xl">
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Nombre de la Empresa</label>
              <input type="text" value={company?.name || ''} onChange={(e) => setCompany((prev) => prev ? { ...prev, name: e.target.value } : null)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">RUC</label>
              <input type="text" value={company?.taxId || ''} onChange={(e) => setCompany((prev) => prev ? { ...prev, taxId: e.target.value } : null)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Telefono</label>
              <input type="text" value={company?.phone || ''} onChange={(e) => setCompany((prev) => prev ? { ...prev, phone: e.target.value } : null)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Email</label>
              <input type="email" value={company?.email || ''} onChange={(e) => setCompany((prev) => prev ? { ...prev, email: e.target.value } : null)} className="input-field" />
            </div>
            <div className="col-span-2">
              <label className="block text-sm font-medium text-slate-700 mb-1">Direccion</label>
              <input type="text" value={company?.address || ''} onChange={(e) => setCompany((prev) => prev ? { ...prev, address: e.target.value } : null)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Impuesto (%)</label>
              <input type="number" value={(company?.taxRate || 0) * 100} onChange={(e) => setCompany((prev) => prev ? { ...prev, taxRate: Number(e.target.value) / 100 } : null)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Moneda</label>
              <input type="text" value={company?.currency || ''} onChange={(e) => setCompany((prev) => prev ? { ...prev, currency: e.target.value } : null)} className="input-field" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Stock Minimo</label>
              <input type="number" value={company?.lowStockThreshold || 10} onChange={(e) => setCompany((prev) => prev ? { ...prev, lowStockThreshold: Number(e.target.value) } : null)} className="input-field" />
            </div>
          </div>
          <button onClick={handleSave} disabled={saving} className="btn-primary flex items-center gap-2">
            <Save className="w-4 h-4" /> {saving ? 'Guardando...' : 'Guardar Cambios'}
          </button>
        </div>
      </div>
    </div>
  )
}
