import React from 'react'

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'warning' | 'success' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  loading?: boolean
  loadingText?: string
  leftIcon?: React.ReactNode
  rightIcon?: React.ReactNode
  fullWidth?: boolean
}

const variantClasses: Record<string, string> = {
  primary: 'bg-gradient-to-br from-indigo-600 to-indigo-700 text-white hover:from-indigo-700 hover:to-indigo-800 active:scale-[0.98] shadow-sm shadow-indigo-200 hover:shadow-md hover:shadow-indigo-300 disabled:opacity-50 disabled:shadow-none',
  secondary: 'bg-slate-100 text-slate-700 hover:bg-slate-200 active:bg-slate-300 active:scale-[0.98] disabled:opacity-50',
  danger: 'bg-gradient-to-br from-red-500 to-red-600 text-white hover:from-red-600 hover:to-red-700 active:scale-[0.98] shadow-sm shadow-red-200 hover:shadow-md hover:shadow-red-300 disabled:opacity-50 disabled:shadow-none',
  warning: 'bg-gradient-to-br from-amber-500 to-amber-600 text-white hover:from-amber-600 hover:to-amber-700 active:scale-[0.98] shadow-sm shadow-amber-200 hover:shadow-md hover:shadow-amber-300 disabled:opacity-50 disabled:shadow-none',
  success: 'bg-gradient-to-br from-emerald-500 to-emerald-600 text-white hover:from-emerald-600 hover:to-emerald-700 active:scale-[0.98] shadow-sm shadow-emerald-200 hover:shadow-md hover:shadow-emerald-300 disabled:opacity-50 disabled:shadow-none',
  ghost: 'bg-transparent text-slate-600 hover:bg-slate-100 active:bg-slate-200 active:scale-[0.98] disabled:opacity-50',
}

const sizeClasses: Record<string, string> = {
  sm: 'px-3 py-1.5 text-sm font-medium rounded-lg',
  md: 'px-4 py-2 text-sm font-semibold rounded-lg',
  lg: 'px-6 py-3 text-base font-semibold rounded-xl',
}

const focusClasses: Record<string, string> = {
  primary: 'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2',
  secondary: 'focus:outline-none focus:ring-2 focus:ring-slate-400 focus:ring-offset-2',
  danger: 'focus:outline-none focus:ring-2 focus:ring-red-400 focus:ring-offset-2',
  warning: 'focus:outline-none focus:ring-2 focus:ring-amber-400 focus:ring-offset-2',
  success: 'focus:outline-none focus:ring-2 focus:ring-emerald-400 focus:ring-offset-2',
  ghost: 'focus:outline-none focus:ring-2 focus:ring-slate-400 focus:ring-offset-2',
}

const Spinner = () => (
  <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
  </svg>
)

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', loading = false, loadingText, leftIcon, rightIcon, fullWidth = false, disabled, children, className, ...props }, ref) => {
    const isDisabled = disabled || loading
    return (
      <button
        ref={ref}
        disabled={isDisabled}
        className={[
          'inline-flex items-center justify-center gap-2 transition-all duration-150 font-medium',
          'disabled:cursor-not-allowed disabled:scale-100',
          variantClasses[variant],
          focusClasses[variant],
          sizeClasses[size],
          fullWidth ? 'w-full' : '',
          className,
        ].filter(Boolean).join(' ')}
        {...props}
      >
        {loading ? <Spinner /> : leftIcon ? <span className="w-4 h-4 flex-shrink-0">{leftIcon}</span> : null}
        <span>{loading ? (loadingText ?? 'Cargando...') : children}</span>
        {!loading && rightIcon && <span className="w-4 h-4 flex-shrink-0">{rightIcon}</span>}
      </button>
    )
  }
)

Button.displayName = 'Button'
