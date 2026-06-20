import { BrowserWindow, ipcMain } from 'electron';

export function setupScanner() {
  let scanBuffer = '';
  let scanTimeout: NodeJS.Timeout | null = null;

  ipcMain.handle('scanner:register', () => {
    const win = BrowserWindow.getFocusedWindow();
    if (!win) return;

    win.webContents.on('before-input-event', (_event, input) => {
      if (input.type === 'keyDown' && input.key.length === 1) {
        scanBuffer += input.key;

        if (scanTimeout) clearTimeout(scanTimeout);

        scanTimeout = setTimeout(() => {
          if (scanBuffer.length > 3) {
            win?.webContents.send('scanner:barcode', scanBuffer);
          }
          scanBuffer = '';
        }, 50);
      }
    });
  });
}
