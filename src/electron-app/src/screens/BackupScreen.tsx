import { useState, useEffect } from 'react'
import { Database, Download, Upload, RefreshCw, CheckCircle, AlertTriangle } from 'lucide-react'
import { toast } from 'sonner'
import { backupService } from '../services/backupService'
import { formatDateTime, formatFileSize } from '../utils/format'
import type { BackupInfo } from '../types/entities'
import { DataTable } from '../components/data-table/DataTable'
import { Modal, ConfirmDialog, FormField } from '../components/ui'

export default function BackupScreen() {
  const [backups, setBackups] = useState<BackupInfo[]>([])
  const [loading, setLoading] = useState(true)
  const [creating, setCreating] = useState(false)
  const [restoreFile, setRestoreFile] = useState<string | null>(null)
  const [verifyInfo, setVerifyInfo] = useState<BackupInfo | null>(null)

  const fetchBackups = async () => {
    setLoading(true)
    try {
      const result = await backupService.getAll()
      if (result.isSuccess) setBackups(result.data || [])
      else toast.error(result.message)
    } catch { } finally { setLoading(false) }
  }

  useEffect(() => { fetchBackups() }, [])

  const handleCreate = async () => {
    setCreating(true)
    try {
      const result = await backupService.create()
      if (result.isSuccess) toast.success('Backup creado exitosamente')
      else toast.error(result.message)
      await fetchBackups()
    } catch {
      toast.error('Error al crear el backup')
    } finally { setCreating(false) }
  }

  const handleRestore = async () => {
    if (!restoreFile) return
    try {
      const result = await backupService.restore(restoreFile)
      if (result.isSuccess) {
        toast.success('Base de datos restaurada exitosamente')
        setRestoreFile(null)
      } else toast.error(result.message)
    } catch {
      toast.error('Error al restaurar el backup')
    }
  }

  const handleVerify = async (fileName: string) => {
    try {
      const result = await backupService.getInfo(fileName)
      if (result.isSuccess && result.data) setVerifyInfo(result.data)
      else toast.error(result.message)
    } catch {
      toast.error('Error al verificar el backup')
    }
  }

  const columns = [
    { header: 'Archivo', accessorKey: 'fileName', cell: ({ row }: any) => <span className="font-mono text-sm">{row.original.fileName}</span> },
    { header: 'Fecha', accessorKey: 'createdAt', cell: ({ row }: any) => <span className="text-sm text-slate-600">{formatDateTime(row.original.createdAt)}</span> },
    { header: 'Tamaño', accessorKey: 'fileSizeBytes', cell: ({ row }: any) => <span className="text-right text-sm">{formatFileSize(row.original.fileSizeBytes)}</span> },
    {
      header: 'Integridad',
      accessorKey: 'hashSha256',
      cell: ({ row }: any) => (
        row.original.hashSha256
          ? <CheckCircle className="w-5 h-5 text-emerald-500 mx-auto" />
          : <AlertTriangle className="w-5 h-5 text-amber-500 mx-auto" />
      ),
    },
    {
      header: 'Acciones',
      id: 'actions',
      cell: ({ row }: any) => (
        <div className="flex items-center justify-center gap-2">
          <button
            onClick={() => setRestoreFile(row.original.fileName)}
            className="p-1 text-slate-400 hover:text-indigo-600"
            title="Restaurar"
          >
            <Upload className="w-4 h-4" />
          </button>
          <button
            onClick={() => handleVerify(row.original.fileName)}
            className="p-1 text-slate-400 hover:text-emerald-600"
            title="Verificar"
          >
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Backups</h1>
        <button onClick={handleCreate} disabled={creating} className="btn-primary flex items-center gap-2">
          <Database className="w-4 h-4" /> {creating ? 'Creando...' : 'Crear Backup'}
        </button>
      </div>

      <div className="card">
        <p className="text-sm text-slate-600 mb-1">
          <span className="font-medium">Ubicación:</span> %APPDATA%/SistemaGVP/Backups
        </p>
        <p className="text-sm text-slate-600">
          <span className="font-medium">Rotación:</span> Se conservan los últimos 30 backups
        </p>
      </div>

      <DataTable columns={columns} data={backups} loading={loading} emptyMessage="Sin backups" />

      <ConfirmDialog
        isOpen={restoreFile !== null}
        onClose={() => setRestoreFile(null)}
        onConfirm={handleRestore}
        title="Restaurar Backup"
        message="¿Está seguro de restaurar este backup? Esto sobrescribirá la base de datos actual y todos los cambios no respaldados se perderán."
        confirmLabel="Restaurar"
        variant="danger"
      />

      <Modal isOpen={verifyInfo !== null} onClose={() => setVerifyInfo(null)} title="Información del Backup" size="sm">
        {verifyInfo && (
          <div className="space-y-3">
            <div>
              <p className="text-sm font-medium text-slate-700">Archivo</p>
              <p className="text-sm text-slate-600 font-mono">{verifyInfo.fileName}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-slate-700">Fecha</p>
              <p className="text-sm text-slate-600">{formatDateTime(verifyInfo.createdAt)}</p>
            </div>
            <div>
              <p className="text-sm font-medium text-slate-700">Tamaño</p>
              <p className="text-sm text-slate-600">{formatFileSize(verifyInfo.fileSizeBytes)}</p>
            </div>
            {verifyInfo.hashSha256 && (
              <div>
                <p className="text-sm font-medium text-slate-700">SHA256</p>
                <p className="text-sm text-slate-600 font-mono break-all">{verifyInfo.hashSha256}</p>
              </div>
            )}
            {verifyInfo.companyName && (
              <div>
                <p className="text-sm font-medium text-slate-700">Empresa</p>
                <p className="text-sm text-slate-600">{verifyInfo.companyName}</p>
              </div>
            )}
            {verifyInfo.createdByUser && (
              <div>
                <p className="text-sm font-medium text-slate-700">Creado por</p>
                <p className="text-sm text-slate-600">{verifyInfo.createdByUser}</p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  )
}
