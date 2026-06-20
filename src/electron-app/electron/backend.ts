import { spawn, ChildProcess } from 'child_process';
import { join } from 'path';
import { existsSync } from 'fs';
import { app } from 'electron';
import http from 'http';

export class BackendManager {
  private process: ChildProcess | null = null;
  private port = 5000;
  private status: 'stopped' | 'starting' | 'running' | 'error' = 'stopped';

  async start(): Promise<void> {
    this.status = 'starting';

    const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;

    // Allow skipping backend start when it's already running externally (e.g. start.bat option 3)
    if (process.env.GVPSKIPBACKEND === '1') {
      this.status = 'running';
      console.log('[Backend] Skipping backend start (GVPSKIPBACKEND=1). Using existing backend on port', this.port);
      return;
    }

    const backendPath = isDev
      ? join(app.getAppPath(), '..', '..', 'src', 'SistemaGVP.API')
      : join(process.resourcesPath, 'backend');

    const backendExe = process.platform === 'win32' ? 'SistemaGVP.API.exe' : 'SistemaGVP.API';

    if (!isDev) {
      const exePath = join(backendPath, backendExe);
      if (!existsSync(exePath)) {
        this.status = 'error';
        throw new Error(`Backend executable not found at ${exePath}. Run the build script first.`);
      }
    }

    const args = isDev
      ? ['run', '--project', backendPath, '--urls', `http://127.0.0.1:${this.port}`]
      : [join(backendPath, backendExe), '--urls', `http://127.0.0.1:${this.port}`];

    this.process = spawn(isDev ? 'dotnet' : args[0], isDev ? args : args.slice(1), {
      stdio: ['pipe', 'pipe', 'pipe'],
      cwd: isDev ? undefined : backendPath,
    });

    this.process.stdout?.on('data', (data) => {
      console.log(`[Backend] ${data}`);
    });

    this.process.stderr?.on('data', (data) => {
      console.error(`[Backend Error] ${data}`);
    });

    this.process.on('exit', (code) => {
      console.log(`[Backend] Process exited with code ${code}`);
      this.status = 'stopped';
    });

    await this.waitForHealth();
  }

  private waitForHealth(): Promise<void> {
    return new Promise((resolve, reject) => {
      let attempts = 0;
      const maxAttempts = 30;

      const check = () => {
        attempts++;
        const req = http.get(`http://127.0.0.1:${this.port}/health`, (res) => {
          if (res.statusCode === 200) {
            this.status = 'running';
            resolve();
          } else if (attempts < maxAttempts) {
            setTimeout(check, 500);
          } else {
            this.status = 'error';
            reject(new Error('Backend failed to start'));
          }
        });

        req.on('error', () => {
          if (attempts < maxAttempts) {
            setTimeout(check, 500);
          } else {
            this.status = 'error';
            reject(new Error('Backend failed to start'));
          }
        });

        req.end();
      };

      check();
    });
  }

  async stop(): Promise<void> {
    if (this.process) {
      // On Windows, SIGTERM is mapped to unconditional kill by Node.js.
      // Use explicit taskkill on Windows, SIGTERM on Unix for clean shutdown.
      if (process.platform === 'win32') {
        this.process.kill('SIGKILL');
      } else {
        this.process.kill('SIGTERM');
      }
      this.process = null;
      this.status = 'stopped';
    }
  }

  getStatus() {
    return this.status;
  }
}
