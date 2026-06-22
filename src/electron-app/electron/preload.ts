import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('electronAPI', {
  getBackendStatus: () => ipcRenderer.invoke('backend:status'),
  getAppVersion: () => ipcRenderer.invoke('app:version'),
  onBackendStatus: (callback: (status: string) => void) => {
    ipcRenderer.on('backend:status-changed', (_event, status) => callback(status));
  },
  printTicket: (html: string) => ipcRenderer.invoke('printer:print', html),
  getLicenseStatus: () => ipcRenderer.invoke('license:status'),
  activateLicense: (licenseKey: string) => ipcRenderer.invoke('license:activate', licenseKey),
  registerScanner: () => ipcRenderer.invoke('scanner:register'),
  onBarcode: (callback: (barcode: string) => void) => {
    ipcRenderer.on('scanner:barcode', (_event, barcode) => callback(barcode));
  },
});
