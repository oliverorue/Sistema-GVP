import { join } from 'path';
import { appendFileSync, existsSync, mkdirSync } from 'fs';
import { BackendManager } from './backend';
import { setupUpdater } from './updater';
import { setupLicenseIPC } from './license';

// Types from electron (used for annotations, not runtime)
import type { App, BrowserWindow as _BrowserWindow, IpcMain, Tray as _Tray, Menu as _Menu, Dialog } from 'electron';

// Startup logging - MUST initialize before require('electron')
// __dirname is inside read-only ASAR, so use a real filesystem path
const logDir = join(process.env.APPDATA || require('os').homedir(), 'SistemaGVP', 'logs');
const logFile = join(logDir, 'gvp-startup.log');
try {
  if (!existsSync(logDir)) mkdirSync(logDir, { recursive: true });
  appendFileSync(logFile, '=== GVP STARTUP ===\n');
  appendFileSync(logFile, `[${new Date().toISOString()}] Module init: __dirname=${__dirname}, cwd=${process.cwd()}\n`);
} catch (e: any) {
  // Ultimate fallback: try desktop
  try { require('fs').writeFileSync(join(require('os').homedir(), 'Desktop', 'gvp-crash.txt'), 'Log init FAILED: ' + (e?.message || e) + '\n'); } catch {}
}
function startupLog(msg: string) {
  try {
    appendFileSync(logFile, `[${new Date().toISOString()}] ${msg}\n`);
  } catch {}
}

// On some Windows systems, require('electron') returns a path string (npm shim)
// instead of the built-in Electron module. Try multiple resolution strategies.
let e: any;
try {
  e = require('electron');
  startupLog(`electron require returned: ${typeof e}`);
  // If it's a string (path), the npm shim was loaded instead of the built-in module.
  // Try to get the real electron module via other means.
  if (typeof e === 'string' || !e.app) {
    startupLog('electron is not the built-in module, trying alternatives...');
    // Method 1: Try process.mainModule.require (Electron internal)
    try { e = (process as any).mainModule?.require?.('electron'); } catch {}
    // Method 2: Try global require from the Electron context
    if (!e?.app) try { e = (global as any).require?.('electron'); } catch {}
    // Method 3: Use dynamic import (ESM)
    if (!e?.app) {
      startupLog('All sync methods failed - electron built-in module unavailable');
    }
  }
} catch (err: any) {
  startupLog(`require('electron') threw: ${err.message}`);
}

const app: App = e?.app;
const BrowserWindow: typeof _BrowserWindow = e?.BrowserWindow;
const ipcMain: IpcMain = e?.ipcMain;
const Tray: typeof _Tray = e?.Tray;
const Menu: typeof _Menu = e?.Menu;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const nativeImage: any = e?.nativeImage;
const dialog: Dialog = e?.dialog;
startupLog(`electron loaded, app type: ${typeof app}`);

let mainWindow: _BrowserWindow | null = null;
let tray: _Tray | null = null;
const backend = new BackendManager();

function createWindow() {
  startupLog('Creating window...');
  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 1024,
    minHeight: 600,
    title: 'Sistema GVP',
    show: false,
    webPreferences: {
      preload: join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
      webSecurity: false,
    },
  });
  startupLog(`Window created, __dirname=${__dirname}`);

  if (process.env.NODE_ENV === 'development') {
    startupLog('Loading dev URL: http://localhost:5173');
    mainWindow.loadURL('http://localhost:5173');
    mainWindow.webContents.openDevTools();
  } else {
    const htmlPath = join(__dirname, '../dist/index.html');
    startupLog(`Loading production file: ${htmlPath}`);
    mainWindow.loadFile(htmlPath);
  }

  mainWindow.once('ready-to-show', () => {
    startupLog('Window ready-to-show, showing...');
    mainWindow?.show();
  });

  mainWindow.on('closed', () => {
    startupLog('Window closed');
    mainWindow = null;
  });

  mainWindow.webContents.on('did-fail-load', (_event: Electron.Event, code: number, desc: string, url: string) => {
    startupLog(`Page load FAILED: code=${code} desc="${desc}" url=${url}`);
  });

  mainWindow.webContents.on('did-finish-load', () => {
    startupLog('Page load finished successfully');
  });

  mainWindow.webContents.on('render-process-gone', (_event: Electron.Event, details: Electron.RenderProcessGoneDetails) => {
    startupLog(`Render process GONE: reason=${details.reason} exitCode=${details.exitCode}`);
  });
}

function createTray() {
  const icon = nativeImage.createEmpty();
  tray = new Tray(icon);
  tray.setToolTip('Sistema GVP');
  const contextMenu = Menu.buildFromTemplate([
    { label: 'Abrir', click: () => mainWindow?.show() },
    { label: 'Cerrar', click: () => app.quit() },
  ]);
  tray.setContextMenu(contextMenu);
}

if (!app) {
  startupLog('FATAL: electron app module not available');
  const msg = 'No se pudo iniciar la aplicacion.\n\nEl modulo de Electron no esta disponible.\n\nVerifique que no haya software de seguridad bloqueando la ejecucion.';
  try { require('electron').dialog?.showErrorBox('Error critico', msg); } catch {}
  process.exit(1);
}

startupLog('========================================');
startupLog('App starting - whenReady...');
startupLog(`isPackaged=${app.isPackaged}, NODE_ENV=${process.env.NODE_ENV}, GVPSKIPBACKEND=${process.env.GVPSKIPBACKEND}`);
startupLog(`resourcesPath=${process.resourcesPath}`);

app.whenReady().then(async () => {
  startupLog('app.whenReady resolved');
  try {
    startupLog('Starting backend...');
    try {
      await backend.start();
      startupLog(`Backend started successfully, status=${backend.getStatus()}`);
    } catch (err: any) {
      const message = err?.message || String(err);
      startupLog(`Backend FAILED: ${message}`);
      console.error('Failed to start backend:', message);
      dialog.showErrorBox('Error al iniciar el backend', `No se pudo iniciar el servidor API:\n\n${message}\n\nVerifique que el backend esté compilado (scripts\\build.bat) y que el puerto 5000 esté libre.`);
      app.quit();
      return;
    }

    createWindow();
    createTray();
    startupLog('Window and tray created');

    try {
      setupUpdater(mainWindow);
      startupLog('Updater setup OK');
    } catch (err: any) {
      startupLog(`Updater setup FAILED: ${err?.message || err}`);
      console.error('Failed to setup updater:', err?.message || err);
    }

    ipcMain.handle('backend:status', () => backend.getStatus());
    ipcMain.handle('app:version', () => app.getVersion());

    try {
      setupLicenseIPC();
      startupLog('License IPC setup OK');
    } catch (err: any) {
      startupLog(`License IPC setup FAILED: ${err?.message || err}`);
      console.error('Failed to setup license IPC:', err?.message || err);
    }
    startupLog('Startup sequence complete');
  } catch (err: any) {
    const message = err?.message || String(err);
    startupLog(`OUTER ERROR: ${message}\n${err?.stack || ''}`);
    console.error('Unhandled startup error:', message, err?.stack);
    dialog.showErrorBox('Error de inicio', `Error inesperado al iniciar la aplicacion:\n\n${message}`);
    app.quit();
  }
});

app.on('before-quit', async () => {
  await backend.stop();
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow();
  }
});
