export interface LicenseStatus {
  valid: boolean
  trial: boolean
  daysLeft?: number
  machineId: string
}

export interface LicenseResult {
  success: boolean
  message: string
}

export interface ElectronAPI {
  getBackendStatus: () => Promise<string>
  getAppVersion: () => Promise<string>
  onBackendStatus: (callback: (status: string) => void) => void
  printTicket: (html: string) => Promise<{ success: boolean; message?: string }>
  htmlToPdf: (html: string) => Promise<{ success: boolean; data?: string; message?: string }>
  getLicenseStatus: () => Promise<LicenseStatus>
  activateLicense: (licenseKey: string) => Promise<LicenseResult>
  registerScanner: () => Promise<void>
  onBarcode: (callback: (barcode: string) => void) => void
}

declare global {
  interface Window {
    electronAPI?: ElectronAPI
  }
}
