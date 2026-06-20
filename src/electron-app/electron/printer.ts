import { ipcMain, BrowserWindow } from 'electron';

export function setupPrinter() {
  ipcMain.handle('printer:print', async (_event, content: string) => {
    try {
      const printWin = new BrowserWindow({
        width: 400,
        height: 600,
        show: false,
        webPreferences: { sandbox: true },
      });

      await printWin.loadURL(`data:text/html;charset=utf-8,${encodeURIComponent(content)}`);

      printWin.webContents.print(
        {
          silent: true,
          printBackground: true,
          deviceName: '',
        },
        (success, errorType) => {
          printWin.close();
          if (!success) {
            console.error('Print failed:', errorType);
          }
        }
      );
      return { success: true };
    } catch (error) {
      return { success: false, message: String(error) };
    }
  });
}
