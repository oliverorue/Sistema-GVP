import type { BrowserWindow } from 'electron';
import { existsSync } from 'fs';
import { join } from 'path';
import { autoUpdater } from 'electron-updater';
import log from 'electron-log';

export function setupUpdater(mainWindow: BrowserWindow | null) {
  const { app } = require('electron');
  // Skip updater in local/unpacked builds where app-update.yml doesn't exist
  // (only generated during full NSIS installer build)
  const updateConfigPath = join(process.resourcesPath, 'app-update.yml');
  if (!existsSync(updateConfigPath)) {
    log.info('[Updater] Skipped - no update config (local build)');
    return;
  }

  autoUpdater.logger = log;
  autoUpdater.autoDownload = false;

  autoUpdater.on('update-available', (info) => {
    mainWindow?.webContents.send('update:available', info);
  });

  autoUpdater.on('download-progress', (progress) => {
    mainWindow?.webContents.send('update:progress', progress);
  });

  autoUpdater.on('update-downloaded', () => {
    mainWindow?.webContents.send('update:ready');
  });

  autoUpdater.on('error', (err) => {
    log.error('[Updater]', err);
  });

  setInterval(() => {
    autoUpdater.checkForUpdates().catch(() => {});
  }, 4 * 60 * 60 * 1000);

  autoUpdater.checkForUpdates().catch(() => {});
}

export function downloadUpdate() {
  autoUpdater.downloadUpdate();
}

export function installUpdate() {
  autoUpdater.quitAndInstall();
}
