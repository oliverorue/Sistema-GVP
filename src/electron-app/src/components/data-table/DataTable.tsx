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
      <div className="card-static overflow-hidden p-0">
        <div className="divide-y divide-slate-100">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="flex gap-4 p-4">
              {columns.map((_, j) => (
                <div key={j} className="h-4 bg-gradient-to-r from-slate-100 to-slate-200 rounded-full animate-pulse flex-1" />
              ))}
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (data.length === 0) {
    return (
      <div className="card-static flex flex-col items-center justify-center py-16 text-slate-400 rounded-2xl">
        <svg className="w-12 h-12 mb-3 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
        </svg>
        <p className="text-sm font-medium">{emptyMessage}</p>
      </div>
    )
  }

  return (
    <div className="card-static overflow-hidden p-0 rounded-2xl border-slate-200/80">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id} className="border-b-2 border-slate-200 bg-gradient-to-r from-slate-50 to-white">
                {headerGroup.headers.map((header) => {
                  const sorted = header.column.getIsSorted()
                  return (
                    <th
                      key={header.id}
                      className="px-5 py-3.5 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider cursor-pointer select-none hover:text-slate-700 transition-colors"
                      onClick={header.column.getToggleSortingHandler()}
                    >
                      <div className="flex items-center gap-1.5">
                        {flexRender(header.column.columnDef.header, header.getContext())}
                        {sorted === 'asc' ? (
                          <ChevronUp className="h-3.5 w-3.5 text-indigo-500" />
                        ) : sorted === 'desc' ? (
                          <ChevronDown className="h-3.5 w-3.5 text-indigo-500" />
                        ) : (
                          <ChevronsUpDown className="h-3.5 w-3.5 text-slate-300" />
                        )}
                      </div>
                    </th>
                  )
                })}
              </tr>
            ))}
          </thead>
          <tbody className="divide-y divide-slate-100">
            {table.getRowModel().rows.map((row, i) => (
              <tr key={row.id} className={`transition-colors duration-150 hover:bg-indigo-50/50 ${i % 2 === 0 ? 'bg-white' : 'bg-slate-50/40'}`}>
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id} className="px-5 py-3 text-slate-700">
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {onPageChange && totalPages > 1 && (
        <div className="flex items-center justify-between px-5 py-3 border-t border-slate-200 bg-slate-50/50">
          <span className="text-xs text-slate-500 font-medium">
            Página {page} de {totalPages}
          </span>
          <div className="flex gap-1.5">
            <button
              onClick={() => onPageChange((page ?? 1) - 1)}
              disabled={!page || page <= 1}
              className="btn-secondary text-xs px-3 py-1.5 rounded-lg disabled:opacity-40"
            >
              ← Anterior
            </button>
            <button
              onClick={() => onPageChange((page ?? 1) + 1)}
              disabled={!page || page >= totalPages}
              className="btn-secondary text-xs px-3 py-1.5 rounded-lg disabled:opacity-40"
            >
              Siguiente →
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
