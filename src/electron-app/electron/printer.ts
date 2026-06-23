import type { IpcMainEvent } from 'electron';

export function setupPrinter() {
  const { ipcMain, BrowserWindow } = require('electron');

  ipcMain.handle('printer:print', async (_event: Electron.IpcMainEvent, content: string) => {
    try {
      const printWin = new BrowserWindow({
        width: 400,
        height: 600,
        show: false,
        webPreferences: { sandbox: true },
      });

      await printWin.loadURL(`data:text/html;charset=utf-8,${encodeURIComponent(content)}`);

      const result = await new Promise<{ success: boolean; errorType?: string }>((resolve) => {
        printWin.webContents.print(
          { silent: true, printBackground: true, deviceName: '' },
          (success: boolean, errorType?: string) => {
            printWin.close();
            resolve({ success, errorType });
          }
        );
      });

      if (!result.success) {
        return { success: false, message: `Error al imprimir: ${result.errorType || 'falló la impresión'}` };
      }
      return { success: true };
    } catch (error) {
      return { success: false, message: String(error) };
    }
  });

  ipcMain.handle('printer:html-to-pdf', async (_event: Electron.IpcMainEvent, content: string) => {
    const pdfWin = new BrowserWindow({
      width: 800,
      height: 600,
      show: false,
      webPreferences: { sandbox: true },
    });
    try {
      await pdfWin.loadURL(`data:text/html;charset=utf-8,${encodeURIComponent(content)}`);
      const pdfBuffer = await pdfWin.webContents.printToPDF({
        printBackground: true,
        preferCSSPageSize: true,
        margins: { top: 0, bottom: 0, left: 0, right: 0 },
      });
      return { success: true, data: pdfBuffer.toString('base64') };
    } catch (error) {
      return { success: false, message: String(error) };
    } finally {
      pdfWin.close();
    }
  });
}
