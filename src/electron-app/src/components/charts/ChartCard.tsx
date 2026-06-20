import type { ReactNode } from 'react'

interface ChartCardProps {
  title: string
  children: ReactNode
}

export function ChartCard({ title, children }: ChartCardProps) {
  return (
    <div className="card">
      <h2 className="text-lg font-semibold text-slate-900 mb-4">{title}</h2>
      <div className="w-full" style={{ minHeight: 300 }}>
        {children}
      </div>
    </div>
  )
}
