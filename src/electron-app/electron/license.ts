import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'fs';
import { join } from 'path';
import crypto from 'crypto';

function getElectron() {
  return require('electron');
}

interface LicenseData {
  machineId: string;
  companyName: string;
  issuedAt: string;
  expiresAt?: string;
  signature: string;
}

interface TrialData {
  firstRun: string;
  signature: string;
}

const TRIAL_DAYS = 30;
const PUBLIC_KEY = `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyPln9B7ttzM4X7h7A1xm
jeCZaU88EhBVLrbWVebWOurJYVeU/WHbsXDVS246tnYhzA3tiPbQTcZW6eeh7Eab
mZosh8MYS2IJdZn/l9dtGqPqHThsIH6m9XvTXTR3xI9EGvhT3SMWLgqJslDU9a1/
ih/i3HzWOuIRYDK8/m4egf8Wun1FFsTzaiGseP/ktB1DKxIhjXh7fyo2tViOpMeL
GtMe4QRTEsP/Udjlz4sQyZM/FXbUDckxHjyFboWqAkN/Y6V/N4uOf72oPaA1KkzG
oeS2bp3hz8wNC7uL0BuW7viiYKq4fZtZifE06gfR7CBAcADSjz01IZ4sjC4nK6ZH
DQIDAQAB
-----END PUBLIC KEY-----`;

function getAppDataPath(): string {
  const path = join(getElectron().app.getPath('appData'), 'SistemaGVP');
  if (!existsSync(path)) {
    mkdirSync(path, { recursive: true });
  }
  return path;
}

function getMachineId(): string {
  const os = require('os');
  const hash = crypto.createHash('sha256');
  hash.update(os.hostname() + os.platform() + os.arch());
  return hash.digest('hex').toUpperCase();
}

function verifySignature(data: string, signature: string): boolean {
  try {
    const verifier = crypto.createVerify('SHA256');
    verifier.update(data);
    return verifier.verify(PUBLIC_KEY, signature, 'base64');
  } catch {
    return false;
  }
}

export function getLicenseStatus(): { valid: boolean; trial: boolean; daysLeft?: number; machineId: string } {
  const machineId = getMachineId();
  const licensePath = join(getAppDataPath(), 'license.dat');

  if (existsSync(licensePath)) {
    try {
      const license: LicenseData = JSON.parse(readFileSync(licensePath, 'utf-8'));

      if (!verifySignature(JSON.stringify({ machineId: license.machineId, companyName: license.companyName, issuedAt: license.issuedAt, expiresAt: license.expiresAt }), license.signature)) {
        return { valid: false, trial: true, machineId };
      }

      if (license.machineId !== machineId) {
        return { valid: false, trial: true, machineId };
      }

      if (license.expiresAt && new Date(license.expiresAt) < new Date()) {
        return { valid: false, trial: false, machineId };
      }

      return { valid: true, trial: false, machineId };
    } catch {
      return { valid: false, trial: true, machineId };
    }
  }

  const trialPath = join(getAppDataPath(), 'trial.dat');
  if (existsSync(trialPath)) {
    try {
      const trial: TrialData = JSON.parse(readFileSync(trialPath, 'utf-8'));
      const firstRun = new Date(trial.firstRun);
      const daysLeft = TRIAL_DAYS - Math.floor((Date.now() - firstRun.getTime()) / (1000 * 60 * 60 * 24));
      return { valid: daysLeft > 0, trial: true, daysLeft: Math.max(0, daysLeft), machineId };
    } catch {
      return { valid: true, trial: true, daysLeft: TRIAL_DAYS, machineId };
    }
  }

  const trialData: TrialData = {
    firstRun: new Date().toISOString(),
    signature: '',
  };
  writeFileSync(trialPath, JSON.stringify(trialData, null, 2));
  return { valid: true, trial: true, daysLeft: TRIAL_DAYS, machineId };
}

export function activateLicense(licenseKey: string): { success: boolean; message: string } {
  try {
    const decoded = Buffer.from(licenseKey, 'base64').toString('utf-8');
    const license: LicenseData = JSON.parse(decoded);

    const dataToVerify = JSON.stringify({
      machineId: license.machineId,
      companyName: license.companyName,
      issuedAt: license.issuedAt,
      expiresAt: license.expiresAt,
    });

    if (!verifySignature(dataToVerify, license.signature)) {
      return { success: false, message: 'Clave de licencia invalida.' };
    }

    writeFileSync(join(getAppDataPath(), 'license.dat'), JSON.stringify(license, null, 2));
    return { success: true, message: 'Licencia activada exitosamente.' };
  } catch {
    return { success: false, message: 'Error al procesar la clave de licencia.' };
  }
}

export function setupLicenseIPC() {
  const ipcMain = getElectron().ipcMain;
  ipcMain.handle('license:status', () => getLicenseStatus());
  ipcMain.handle('license:activate', (_event: Electron.IpcMainEvent, licenseKey: string) => activateLicense(licenseKey));
}
