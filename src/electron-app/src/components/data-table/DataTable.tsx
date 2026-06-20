import { useState } from 'react'
import {
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
  type SortingState,
  type ColumnDef,
} from '@tanstack/react-table'
import { ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-react'

interface DataTableProps<T> {
  columns: ColumnDef<T>[]
  data: T[]
  loading?: boolean
  emptyMessage?: string
  page?: number
  totalPages?: number
  onPageChange?: (page: number) => void
}

export function DataTable<T>({
  columns,
  data,
  loading = false,
  emptyMessage = 'No se encontraron registros',
  page,
  totalPages = 1,
  onPageChange,
}: DataTableProps<T>) {
  const [sorting, setSorting] = useState<SortingState>([])

  const table = useReactTable({
    data,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  })

  if (loading) {
    return (
      <div className="card overflow-hidden p-0">
        <div className="divide-y divide-slate-200">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="flex gap-4 p-4">
              {columns.map((_, j) => (
                <div key={j} className="h-4 bg-slate-200 rounded animate-pulse flex-1" />
              ))}
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (data.length === 0) {
    return (
      <div className="card flex flex-col items-center justify-center py-12 text-slate-400">
        <p className="text-sm">{emptyMessage}</p>
      </div>
    )
  }

  return (
    <div className="card overflow-hidden p-0">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id} className="border-b border-slate-200 bg-slate-50">
                {headerGroup.headers.map((header) => {
                  const sorted = header.column.getIsSorted()
                  return (
                    <th
                      key={header.id}
                      className="px-4 py-3 text-left font-medium text-slate-600 cursor-pointer select-none hover:text-slate-900 transition-colors"
                      onClick={header.column.getToggleSortingHandler()}
                    >
                      <div className="flex items-center gap-1">
                        {flexRender(header.column.columnDef.header, header.getContext())}
                        {sorted === 'asc' ? (
                          <ChevronUp className="h-4 w-4" />
                        ) : sorted === 'desc' ? (
                          <ChevronDown className="h-4 w-4" />
                        ) : (
                          <ChevronsUpDown className="h-4 w-4 text-slate-300" />
                        )}
                      </div>
                    </th>
                  )
                })}
              </tr>
            ))}
          </thead>
          <tbody className="divide-y divide-slate-100">
            {table.getRowModel().rows.map((row) => (
              <tr key={row.id} className="hover:bg-slate-50 transition-colors">
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id} className="px-4 py-3 text-slate-700">
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {onPageChange && totalPages > 1 && (
        <div className="flex items-center justify-between px-4 py-3 border-t border-slate-200 bg-slate-50">
          <span className="text-sm text-slate-600">
            Página {page} de {totalPages}
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => onPageChange((page ?? 1) - 1)}
              disabled={!page || page <= 1}
              className="btn-secondary text-sm px-3 py-1.5"
            >
              Anterior
            </button>
            <button
              onClick={() => onPageChange((page ?? 1) + 1)}
              disabled={!page || page >= totalPages}
              className="btn-secondary text-sm px-3 py-1.5"
            >
              Siguiente
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
