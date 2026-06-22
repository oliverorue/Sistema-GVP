/**
 * Componente Button reutilizable con variantes
 * 
 * Uso:
 * <Button variant="primary" size="md">Guardar</Button>
 * <Button variant="danger" loading>Eliminando...</Button>
 */

import React from 'react';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  /**
   * Variante visual del botón
   * @default 'primary'
   */
  variant?: 'primary' | 'secondary' | 'danger' | 'warning' | 'success' | 'ghost';

  /**
   * Tamaño del botón
   * @default 'md'
   */
  size?: 'sm' | 'md' | 'lg';

  /**
   * Mostrar estado de carga
   * @default false
   */
  loading?: boolean;

  /**
   * Texto a mostrar mientras carga
   * @default '⏳ Procesando...'
   */
  loadingText?: string;

  /**
   * Ícono a la izquierda del texto (HTML o componente)
   */
  leftIcon?: React.ReactNode;

  /**
   * Ícono a la derecha del texto
   */
  rightIcon?: React.ReactNode;

  /**
   * Ancho completo del contenedor
   * @default false
   */
  fullWidth?: boolean;
}

const variantClasses: Record<string, string> = {
  primary:
    'bg-blue-600 text-white hover:bg-blue-700 active:bg-blue-800 disabled:bg-gray-400 dark:bg-blue-700 dark:hover:bg-blue-800',
  secondary:
    'bg-gray-200 text-gray-900 hover:bg-gray-300 active:bg-gray-400 disabled:bg-gray-200 dark:bg-gray-700 dark:text-gray-100 dark:hover:bg-gray-600',
  danger:
    'bg-red-600 text-white hover:bg-red-700 active:bg-red-800 disabled:bg-gray-400 dark:bg-red-700 dark:hover:bg-red-800',
  warning:
    'bg-yellow-500 text-white hover:bg-yellow-600 active:bg-yellow-700 disabled:bg-gray-400 dark:bg-yellow-600 dark:hover:bg-yellow-700',
  success:
    'bg-green-600 text-white hover:bg-green-700 active:bg-green-800 disabled:bg-gray-400 dark:bg-green-700 dark:hover:bg-green-800',
  ghost:
    'bg-transparent text-gray-700 hover:bg-gray-100 active:bg-gray-200 dark:text-gray-300 dark:hover:bg-gray-700',
};

const sizeClasses: Record<string, string> = {
  sm: 'px-3 py-1.5 text-sm font-medium',
  md: 'px-4 py-2 text-base font-medium',
  lg: 'px-6 py-3 text-lg font-medium',
};

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = 'primary',
      size = 'md',
      loading = false,
      loadingText = '⏳ Procesando...',
      leftIcon,
      rightIcon,
      fullWidth = false,
      disabled,
      children,
      className,
      ...props
    },
    ref
  ) => {
    const baseClasses =
      'inline-flex items-center justify-center gap-2 rounded-lg transition-colors duration-200 font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:cursor-not-allowed';

    const computedDisabled = disabled || loading;

    const finalClassName = [
      baseClasses,
      variantClasses[variant],
      sizeClasses[size],
      fullWidth && 'w-full',
      className,
    ]
      .filter(Boolean)
      .join(' ');

    return (
      <button
        ref={ref}
        disabled={computedDisabled}
        className={finalClassName}
        {...props}
      >
        {!loading && leftIcon && <span>{leftIcon}</span>}
        {loading && (
          <span className="inline-block animate-spin text-lg">⟳</span>
        )}
        <span>
          {loading ? loadingText : children}
        </span>
        {!loading && rightIcon && <span>{rightIcon}</span>}
      </button>
    );
  }
);

Button.displayName = 'Button';
