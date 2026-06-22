export enum LogLevel {
  DEBUG = 'DEBUG',
  INFO = 'INFO',
  WARN = 'WARN',
  ERROR = 'ERROR',
}

interface LogEntry {
  timestamp: string
  level: LogLevel
  module: string
  message: string
  data?: unknown
  stack?: string
}

class AppLogger {
  private logs: LogEntry[] = []
  private maxLogs = 1000
  private isDev = import.meta.env.DEV

  private addLog(entry: LogEntry): void {
    this.logs.push(entry)
    if (this.logs.length > this.maxLogs) {
      this.logs = this.logs.slice(-this.maxLogs)
    }
  }

  info(module: string, message: string, data?: unknown): void {
    const entry = { timestamp: new Date().toISOString(), level: LogLevel.INFO, module, message, data }
    this.addLog(entry)
    if (this.isDev) console.log(`[${entry.timestamp}] [INFO] [${module}] ${message}`, data ?? '')
  }

  error(module: string, message: string, error?: Error | unknown, extraData?: Record<string, unknown>): void {
    const entry = {
      timestamp: new Date().toISOString(),
      level: LogLevel.ERROR,
      module,
      message,
      data: { errorMessage: error instanceof Error ? error.message : String(error ?? ''), ...extraData },
      stack: error instanceof Error ? error.stack : undefined,
    }
    this.addLog(entry)
    console.error(`[${entry.timestamp}] [ERROR] [${module}] ${message}`, entry.data ?? '')
    if (entry.stack && this.isDev) console.error('Stack:', entry.stack)
  }

  warn(module: string, message: string, data?: unknown): void {
    const entry = { timestamp: new Date().toISOString(), level: LogLevel.WARN, module, message, data }
    this.addLog(entry)
    if (this.isDev) console.warn(`[${entry.timestamp}] [WARN] [${module}] ${message}`, data ?? '')
  }

  debug(module: string, message: string, data?: unknown): void {
    if (!this.isDev) return
    const entry = { timestamp: new Date().toISOString(), level: LogLevel.DEBUG, module, message, data }
    this.addLog(entry)
    console.debug(`[${entry.timestamp}] [DEBUG] [${module}] ${message}`, data ?? '')
  }

  getLogs(): LogEntry[] { return [...this.logs] }
  exportLogs(): string { return JSON.stringify(this.logs, null, 2) }
  getLogsByModule(module: string): LogEntry[] { return this.logs.filter(l => l.module === module) }
  getLogsByLevel(level: LogLevel): LogEntry[] { return this.logs.filter(l => l.level === level) }
  clear(): void { this.logs = [] }
}

export const Logger = new AppLogger()
