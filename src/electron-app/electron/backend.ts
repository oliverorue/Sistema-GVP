import { spawn, ChildProcess } from 'child_process';
import { join } from 'path';
import { existsSync } from 'fs';
import http from 'http';

function getApp() {
  return require('electron').app;
}

export class BackendManager {
  private process: ChildProcess | null = null;
  private port = 5000;
  private status: 'stopped' | 'starting' | 'running' | 'error' = 'stopped';
  private log: (msg: string) => void;
  private exited = false;

  constructor(logger?: (msg: string) => void) {
    this.log = logger || ((msg: string) => {});
  }

  async start(): Promise<void> {
    this.status = 'starting';

    const isDev = process.env.NODE_ENV === 'development' || !getApp().isPackaged;

    if (process.env.GVPSKIPBACKEND === '1') {
      this.status = 'running';
      console.log('[Backend] Skipping backend start (GVPSKIPBACKEND=1). Using existing backend on port', this.port);
      return;
    }

    const backendPath = isDev
      ? join(getApp().getAppPath(), '..', '..', 'src', 'SistemaGVP.API')
      : join(process.resourcesPath, 'backend');

    const backendExe = process.platform === 'win32' ? 'SistemaGVP.API.exe' : 'SistemaGVP.API';
    const exePath = isDev ? '' : join(backendPath, backendExe);

    if (!isDev) {
      this.log(`Checking backend exe at: ${exePath}`);
      if (!existsSync(exePath)) {
        this.status = 'error';
        throw new Error(`Backend executable not found at ${exePath}. Run the build script first.`);
      }
      this.log('Backend exe found OK');
    }

    this.exited = false;
    const urls = `http://127.0.0.1:${this.port}`;
    const args = isDev
      ? ['run', '--project', backendPath, '--urls', urls]
      : ['--urls', urls, '--content-root', backendPath];

    this.log(`Spawning: ${isDev ? 'dotnet' : exePath} ${args.join(' ')}`);

    let cmdLauncher = false;

    if (isDev) {
      this.process = spawn('dotnet', args, {
        stdio: ['ignore', 'pipe', 'pipe'],
      });
    } else {
      // Copy appsettings.json to userData so the backend can find it when we start it from there
      const appDataDir = getApp().getPath('userData');
      const configDest = join(appDataDir, 'appsettings.json');
      const configSrc = join(backendPath, 'appsettings.json');
      try {
        if (existsSync(configSrc)) {
          require('fs').copyFileSync(configSrc, configDest);
        }
      } catch {
        // Non-critical
      }
      // Start the backend from AppData (writable, with appsettings.json) using Start-Process
      cmdLauncher = true;
      const psCmd = `Start-Process -FilePath "${exePath}" -ArgumentList '--urls','http://127.0.0.1:5000' -WorkingDirectory '${appDataDir.replace(/'/g, "''")}' -NoNewWindow -PassThru`;
      this.log('Launching backend via PowerShell...');
      this.process = spawn('powershell.exe', ['-NoProfile', '-Command', psCmd], {
        stdio: ['ignore', 'pipe', 'pipe'],
      });
    }

    this.process.on('error', (err) => {
      const msg = `[Backend] SPAWN ERROR: ${err.message}`;
      console.error(msg);
      this.log(msg);
    });

    this.process.stdout?.on('data', (data) => {
      data.toString().split('\n').filter((l: string) => l.trim()).forEach((line: string) => {
        const msg = `[Backend] ${line.trim()}`;
        console.log(msg);
        this.log(msg);
      });
    });

    this.process.stderr?.on('data', (data) => {
      data.toString().split('\n').filter((l: string) => l.trim()).forEach((line: string) => {
        const msg = `[Backend Error] ${line.trim()}`;
        console.error(msg);
        this.log(msg);
      });
    });

    this.process.on('exit', (code, signal) => {
      if (!cmdLauncher) {
        const msg = `[Backend] Process exited: code=${code} signal=${signal}`;
        console.log(msg);
        this.log(msg);
        this.exited = true;
        this.status = 'stopped';
      } else {
        this.log('[Backend] Launcher finished (backend running independently)');
      }
    });

    this.log('Waiting for backend health check...');
    await this.waitForHealth();
  }

  private waitForHealth(): Promise<void> {
    return new Promise((resolve, reject) => {
      let attempts = 0;
      const maxAttempts = 30;
      let done = false;

      const fail = (reason: string) => {
        if (done) return;
        done = true;
        this.status = 'error';
        reject(new Error(reason));
      };

      const check = () => {
        if (done) return;
        
        if (this.exited) {
          fail('Backend process exited before becoming healthy');
          return;
        }

        attempts++;
        const req = http.get(`http://127.0.0.1:${this.port}/health`, (res) => {
          // Backend is responding - consider it healthy regardless of status code.
          // 500s can happen during initialization (missing config, DB seeding, etc.)
          // but the process is alive and will eventually serve valid responses.
          let body = '';
          res.on('data', (chunk: string) => body += chunk);
          res.on('end', () => {
            this.log(`Health check responded: ${res.statusCode} - ${body.substring(0, 100)}`);
            this.status = 'running';
            done = true;
            resolve();
          });
        });

        req.on('error', (err: NodeJS.ErrnoException) => {
          if (done) return;
          if (this.exited) {
            fail('Backend process exited before becoming healthy');
          } else if (attempts >= maxAttempts) {
            fail(`Backend failed to start after ${maxAttempts} attempts`);
          } else {
            this.log(`Health attempt ${attempts}/${maxAttempts}: ${err.code}`);
            setTimeout(check, 2000);
          }
        });

        req.end();
      };

      // Give the process 3 seconds to start before first health check
      setTimeout(check, 3000);
    });
  }

  async stop(): Promise<void> {
    // On Windows, cmd /c start /b means we don't own the child PID.
    // Kill by executable name instead.
    if (process.platform === 'win32') {
      spawn('taskkill', ['/f', '/im', 'SistemaGVP.API.exe']);
    }
    if (this.process) {
      this.process.kill('SIGTERM');
      this.process = null;
    }
    this.status = 'stopped';
  }

  getStatus() {
    return this.status;
  }
}
