export class AppError extends Error {
  constructor(
    public code: string,
    message: string,
    public statusCode?: number,
    public context?: Record<string, unknown>
  ) {
    super(message)
    this.name = 'AppError'
  }
}

export class ApiError extends AppError {
  constructor(
    public endpoint: string,
    statusCode: number,
    message: string,
    public originalError?: unknown
  ) {
    super(`API_ERROR_${statusCode}`, message, statusCode, { endpoint })
    this.name = 'ApiError'
  }

  getUserMessage(): string {
    const messages: Record<number, string> = {
      400: 'Datos inválidos. Revisa los campos.',
      401: 'Sesión expirada. Inicia sesión nuevamente.',
      403: 'No tienes permiso para esta acción.',
      404: 'El recurso no fue encontrado.',
      409: 'El recurso ya existe o hay un conflicto.',
      500: 'Error en el servidor. Intenta más tarde.',
      503: 'El servicio no está disponible.',
    }
    return messages[this.statusCode || 500] || this.message
  }
}

export class ValidationError extends AppError {
  constructor(public field: string, message: string, public value?: unknown) {
    super(`VALIDATION_ERROR_${field}`, message, 400, { field, value })
    this.name = 'ValidationError'
  }
}

export class NetworkError extends AppError {
  constructor(message = 'Error de conexión') {
    super('NETWORK_ERROR', message, undefined, {})
    this.name = 'NetworkError'
  }
}

export class AuthenticationError extends AppError {
  constructor(message = 'Autenticación requerida') {
    super('AUTH_ERROR', message, 401)
    this.name = 'AuthenticationError'
  }
}

export class TimeoutError extends AppError {
  constructor(operation: string) {
    super('TIMEOUT_ERROR', `${operation} tardó demasiado`, undefined, { operation })
    this.name = 'TimeoutError'
  }
}

export const errorUtils = {
  getUserMessage(error: unknown): string {
    if (error instanceof ApiError) return error.getUserMessage()
    if (error instanceof ValidationError) return `Campo "${error.field}": ${error.message}`
    if (error instanceof NetworkError) return 'Verifica tu conexión a internet.'
    if (error instanceof AuthenticationError) return 'Debes iniciar sesión.'
    if (error instanceof TimeoutError) return 'La operación tardó demasiado. Intenta nuevamente.'
    if (error instanceof Error) return error.message
    return 'Ocurrió un error desconocido.'
  },

  fromResponse(response: Response, originalError?: unknown): ApiError {
    return new ApiError(response.url, response.status, response.statusText, originalError)
  },

  fromNetworkError(error: unknown): NetworkError {
    const msg = error instanceof Error ? error.message : 'Error de conexión al servidor'
    return new NetworkError(msg)
  },
}
