import { useState, useEffect } from 'react'
import { Save } from 'lucide-react'
import { settingsService } from '../services/settingsService'
import { Company } from '../types/entities'
import { Logger } from '../utils/logger'

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
        Logger.error('SettingsScreen', 'Error al cargar configuracion', err)
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
      Logger.error('SettingsScreen', 'Error al guardar configuracion', err)
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
          <div className="mt-4">
            <label className="block text-sm font-medium text-slate-700 mb-2">IVA en precios</label>
            <label className="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" checked={company?.ivaIncluido ?? true}
                onChange={(e) => setCompany(prev => prev ? {...prev, ivaIncluido: e.target.checked} : null)}
                className="sr-only peer" />
              <div className="w-11 h-6 bg-slate-200 peer-focus:ring-2 peer-focus:ring-indigo-300 rounded-full peer peer-checked:bg-indigo-600 peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
              <span className="ml-3 text-sm text-slate-700">IVA incluido en precios</span>
            </label>
            <p className="text-xs text-slate-400 mt-1">Si esta activo, los precios en gondola ya incluyen el IVA (10%)</p>
          </div>
          <button onClick={handleSave} disabled={saving} className="btn-primary flex items-center gap-2">
            <Save className="w-4 h-4" /> {saving ? 'Guardando...' : 'Guardar Cambios'}
          </button>
        </div>
      </div>
    </div>
  )
}
