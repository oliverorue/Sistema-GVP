using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SistemaGVP.WPF.Services;

/// <summary>
/// Servidor HTTP embebido que expone una página con escáner de código de barras
/// para usar desde el celular. Escanea con la cámara y envía el código al POS.
/// Usa Kestrel con HTTPS autofirmado para permitir acceso a la cámara del dispositivo móvil.
/// </summary>
public sealed class BarcodeHttpServer : IDisposable
{
    private readonly ILogger<BarcodeHttpServer>? _logger;
    private IWebHost? _webHost;

    private const int Port = 5180;

    /// <summary>
    /// Evento disparado cuando se recibe un código de barras escaneado desde el celular.
    /// </summary>
    public event EventHandler<string>? BarcodeScanned;

    /// <summary>
    /// URL que debe ingresar el usuario en su celular (conectado a la misma red WiFi).
    /// Siempre usa HTTPS con la IP LAN local, necesario para que el navegador móvil
    /// permita acceso a la cámara (getUserMedia requiere HTTPS).
    /// </summary>
    public string ServerUrl => $"https://{GetLocalIpAddress()}:{Port}";

    /// <summary>
    /// Códigos QR generados recientemente (para mostrar en UI).
    /// </summary>
    public ConcurrentQueue<string> RecentScans { get; } = new();

    public BarcodeHttpServer(ILogger<BarcodeHttpServer>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Inicia el servidor Kestrel con HTTPS autofirmado.
    /// Retorna true si el inicio fue exitoso.
    /// </summary>
    public async Task<bool> StartAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[DIAG] BarcodeHttpServer.StartAsync() called. _webHost is null? {_webHost == null}");
        if (_webHost != null)
        {
            System.Diagnostics.Debug.WriteLine("[DIAG] WebHost already exists, returning true");
            return true;
        }

        try
        {
            var cert = CertificateGeneratorService.GetOrCreateCertificate();

            _webHost = new WebHostBuilder()
                .UseKestrel(o =>
                {
                    o.Listen(IPAddress.Any, Port, listen => listen.UseHttps(cert));
                })
                .ConfigureServices(services =>
                {
                    services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
                })
                .Configure(app =>
                {
                    app.UseCors();
                    app.Run(HandleRequest);
                })
                .Build();

            await _webHost.StartAsync();

            var url = ServerUrl;
            _logger?.LogInformation("BarcodeHttpServer (Kestrel+HTTPS) URL: {Url}", url);
            System.Diagnostics.Debug.WriteLine($"[DIAG] BarcodeHttpServer URL: {url}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DIAG] BarcodeHttpServer.StartAsync() FAILED: {ex.GetType().Name}: {ex.Message}");
            _logger?.LogWarning("No se pudo iniciar BarcodeHttpServer en puerto {Port}: {Message}", Port, ex.Message);
            _logger?.LogError(ex, "No se pudo iniciar BarcodeHttpServer en puerto {Port}", Port);

            if (_webHost != null)
            {
                try { _webHost.Dispose(); } catch { }
                _webHost = null;
            }

            return false;
        }
    }

    /// <summary>
    /// Detiene el servidor de forma asíncrona.
    /// </summary>
    public async Task StopAsync()
    {
        if (_webHost != null)
        {
            try
            {
                await _webHost.StopAsync();
                _webHost.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error al detener BarcodeHttpServer");
            }
            _webHost = null;
            _logger?.LogInformation("BarcodeHttpServer detenido");
        }
    }

    /// <summary>
    /// Versión síncrona obsoleta. Usar StopAsync() en su lugar.
    /// </summary>
    [Obsolete("Usar StopAsync() en su lugar")]
    public void Stop()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        // En Dispose() usamos Task.Run para evitar deadlock en contextos de sincronización WPF,
        // con timeout de 5 segundos por seguridad.
        try
        {
            var task = Task.Run(() => StopAsync());
            if (!task.Wait(TimeSpan.FromSeconds(5)))
            {
                _logger?.LogWarning("Timeout al detener BarcodeHttpServer en Dispose()");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al detener BarcodeHttpServer en Dispose()");
        }
    }

    private async Task HandleRequest(HttpContext context)
    {
        if (context.Request.Method == "POST" && context.Request.Path == "/scan")
        {
            using var reader = new StreamReader(context.Request.Body);
            var barcode = await reader.ReadToEndAsync();
            BarcodeScanned?.Invoke(this, barcode.Trim());
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("""{"status":"ok"}""");
            return;
        }

        var html = context.Request.Path == "/content"
            ? GetScannerPageHtml()
            : GetScannerPageHtml(); // "/" también devuelve scanner page

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html);
    }

    private static string GetScannerPageHtml()
    {
        return @"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
    <title>Escáner GVP</title>
    <script src=""https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js""></script>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: #0a0a0a;
            color: #fff;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
        }
        .header {
            background: linear-gradient(135deg, #1565C0, #0D47A1);
            width: 100%;
            padding: 16px 20px;
            text-align: center;
            box-shadow: 0 2px 8px rgba(0,0,0,0.3);
        }
        .header h1 { font-size: 20px; font-weight: 600; }
        .header p { font-size: 13px; opacity: 0.85; margin-top: 4px; }
        .scanner-container {
            width: 100%;
            max-width: 500px;
            padding: 16px;
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
        }
        #reader {
            width: 100% !important;
            min-width: 300px !important;
            min-height: 300px !important;
            max-width: 400px !important;
            border-radius: 12px !important;
            overflow: hidden !important;
            border: 2px solid #333 !important;
            background: #000 !important;
        }
        #reader video { border-radius: 10px !important; }
        #reader__dashboard_section { background: transparent !important; }
        #reader__dashboard_section button {
            background: #1565C0 !important;
            color: #fff !important;
            border: none !important;
            padding: 10px 24px !important;
            border-radius: 8px !important;
            font-size: 14px !important;
            cursor: pointer !important;
        }
        #reader__scan_region { min-height: 220px !important; }
        #flashBtn {
            display: none;
            margin-top: 12px;
            padding: 12px 24px;
            border-radius: 10px;
            border: none;
            background: #333;
            color: #fff;
            font-size: 15px;
            font-weight: 600;
            cursor: pointer;
            transition: background 0.2s;
        }
        #flashBtn:active { opacity: 0.8; }
        #flashBtn.flash-on {
            background: #F9A825;
            color: #1a1a1a;
        }
        .result-card {
            width: 100%;
            margin-top: 16px;
            background: #1a1a2e;
            border-radius: 12px;
            padding: 16px;
            text-align: center;
            border: 1px solid #333;
        }
        .result-card .label { font-size: 12px; color: #888; text-transform: uppercase; letter-spacing: 1px; }
        .result-card .barcode {
            font-size: 22px;
            font-weight: bold;
            color: #4CAF50;
            margin-top: 8px;
            word-break: break-all;
        }
        .manual-input {
            width: 100%;
            margin-top: 12px;
        }
        .manual-input input {
            width: 100%;
            padding: 14px 16px;
            border-radius: 10px;
            border: 1px solid #333;
            background: #1a1a2e;
            color: #fff;
            font-size: 18px;
            text-align: center;
            outline: none;
        }
        .manual-input input:focus { border-color: #1565C0; }
        .manual-input button {
            width: 100%;
            margin-top: 8px;
            padding: 14px;
            border-radius: 10px;
            border: none;
            background: #1565C0;
            color: #fff;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
        }
        .manual-input button:active { background: #0D47A1; }
        .status {
            margin-top: 8px;
            padding: 8px 16px;
            border-radius: 8px;
            font-size: 13px;
            text-align: center;
            min-height: 36px;
        }
        .status.success { background: #1b5e20; color: #a5d6a7; }
        .status.error { background: #b71c1c; color: #ef9a9a; }
        .footer {
            width: 100%;
            padding: 12px;
            text-align: center;
            font-size: 11px;
            color: #555;
        }
        @media (max-width: 480px) {
            .header h1 { font-size: 17px; }
            .result-card .barcode { font-size: 18px; }
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>📷 Escáner GVP</h1>
        <p>Apunta la cámara al código de barras</p>
    </div>

    <div class=""scanner-container"">
        <div id=""reader""></div>

        <button id=""flashBtn"" onclick=""toggleFlash()"">🔦 Flash</button>

        <div id=""cameraWarning"" style=""display:none;"" class=""result-card"">
            <div style=""font-size:32px; margin-bottom:12px;"">⚠️</div>
            <div style=""color:#ffa726; font-weight:600; font-size:17px; margin-bottom:6px;"">Cámara no disponible</div>
            <div style=""color:#aaa; font-size:14px;"">Verificá que hayas aceptado el certificado de seguridad en tu navegador.</div>
        </div>

        <div class=""result-card"" id=""resultCard"" style=""display:none;"">
            <div class=""label"">Último código escaneado</div>
            <div class=""barcode"" id=""lastBarcode"">-</div>
        </div>

        <div class=""manual-input"">
            <input type=""text"" id=""manualBarcode"" inputmode=""numeric"" placeholder=""O ingresa el código manualmente"" />
            <button onclick=""sendManualBarcode()"">📤 Enviar</button>
        </div>

        <div id=""status"" class=""status""></div>
    </div>

    <div class=""footer"">
        Sistema GVP POS — Escáner Móvil
    </div>

    <script>
        let lastScanned = '';
        let scanning = false;
        let html5QrCode = null;
        let flashOn = false;
        let retryCount = 0;
        const MAX_RETRIES = 3;
        let retryTimer = null;
        let scanSuccessReceived = false;

        function showStatus(msg, type) {
            const el = document.getElementById('status');
            el.textContent = msg;
            el.className = 'status ' + (type || '');
            setTimeout(() => { if (el.textContent === msg) { el.textContent = ''; el.className = 'status'; } }, 3000);
        }

        function sendBarcode(barcode) {
            if (!barcode || barcode === lastScanned) return;
            lastScanned = barcode;
            scanSuccessReceived = true;
            if (retryTimer) { clearTimeout(retryTimer); retryTimer = null; }

            fetch('/scan', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ barcode: barcode })
            })
            .then(r => r.json())
            .then(data => {
                document.getElementById('lastBarcode').textContent = barcode;
                document.getElementById('resultCard').style.display = 'block';
                showStatus('✅ Código enviado al POS', 'success');
            })
            .catch(err => {
                showStatus('❌ Error de conexión: ' + err.message, 'error');
            });
        }

        function sendManualBarcode() {
            const input = document.getElementById('manualBarcode');
            const barcode = input.value.trim();
            if (!barcode) { showStatus('⚠️ Ingresa un código', 'error'); return; }
            input.value = '';
            sendBarcode(barcode);
        }

        document.getElementById('manualBarcode').addEventListener('keydown', function(e) {
            if (e.key === 'Enter') sendManualBarcode();
        });

        // Verificar si la cámara está disponible (requiere HTTPS o localhost)
        var cameraAvailable = !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia);

        if (!cameraAvailable) {
            document.getElementById('reader').style.display = 'none';
            document.getElementById('cameraWarning').style.display = 'block';
            document.getElementById('status').textContent = '⚠️ Cámara no disponible. Aceptá el certificado y recargá la página.';
            document.getElementById('status').className = 'status';
        }

        function toggleFlash() {
            if (!html5QrCode) return;
            flashOn = !flashOn;
            try {
                html5QrCode.applyVideoConstraints({
                    advanced: [{ torch: flashOn }]
                }).then(() => {
                    const btn = document.getElementById('flashBtn');
                    if (flashOn) {
                        btn.textContent = '🔆 Flash ON';
                        btn.className = 'flash-on';
                    } else {
                        btn.textContent = '🔦 Flash';
                        btn.className = '';
                    }
                }).catch(() => {
                    // Navegador no soporta torch, revertir estado
                    flashOn = !flashOn;
                });
            } catch (e) {
                flashOn = !flashOn;
            }
        }

        function buildConfig(relaxed) {
            var cfg = {
                fps: 10,
                qrbox: { width: 300, height: 300 },
                aspectRatio: 1,
                disableFlip: false,
                formatsToSupport: [
                    Html5QrcodeSupportedFormats.EAN_13,
                    Html5QrcodeSupportedFormats.EAN_8,
                    Html5QrcodeSupportedFormats.UPC_A,
                    Html5QrcodeSupportedFormats.UPC_E,
                    Html5QrcodeSupportedFormats.CODE_128,
                    Html5QrcodeSupportedFormats.CODE_39,
                    Html5QrcodeSupportedFormats.CODE_93,
                    Html5QrcodeSupportedFormats.ITF,
                    Html5QrcodeSupportedFormats.CODABAR,
                    Html5QrcodeSupportedFormats.QR_CODE,
                    Html5QrcodeSupportedFormats.DATA_MATRIX,
                    Html5QrcodeSupportedFormats.AZTEC,
                    Html5QrcodeSupportedFormats.PDF_417,
                    Html5QrcodeSupportedFormats.MAXICODE,
                    Html5QrcodeSupportedFormats.RSS_14,
                    Html5QrcodeSupportedFormats.RSS_EXPANDED
                ]
            };

            if (relaxed) {
                cfg.videoConstraints = {
                    facingMode: ""environment"",
                    width: { ideal: 640 },
                    height: { ideal: 480 }
                };
            } else {
                cfg.videoConstraints = {
                    facingMode: ""environment"",
                    width: { ideal: 1280 },
                    height: { ideal: 720 }
                };
            }

            return cfg;
        }

        function startScanner(relaxed) {
            if (!cameraAvailable) return;
            if (scanning) return;
            scanning = true;
            scanSuccessReceived = false;

            if (!html5QrCode) {
                html5QrCode = new Html5Qrcode(""reader"");
            }

            var config = buildConfig(relaxed || false);

            html5QrCode.start(
                { facingMode: ""environment"" },
                config,
                function(decodedText) {
                    sendBarcode(decodedText);
                },
                function(errorMessage) {
                    // Ignorar errores intermedios del scanner
                }
            ).then(function() {
                // Cámara iniciada correctamente
                retryCount = 0;
                document.getElementById('flashBtn').style.display = 'inline-block';

                // Configurar reintento automático si no se escanea nada en 10 segundos
                if (retryTimer) clearTimeout(retryTimer);
                retryTimer = setTimeout(function() {
                    if (!scanSuccessReceived && retryCount < MAX_RETRIES) {
                        retryCount++;
                        showStatus('⏳ Reintentando escaneo (' + retryCount + '/' + MAX_RETRIES + ')...', 'error');
                        restartScanner();
                    } else if (!scanSuccessReceived) {
                        showStatus('⚠️ No se detectó código. Probá con entrada manual.', 'error');
                    }
                }, 10000);
            }).catch(function(err) {
                showStatus('❌ Error al iniciar cámara: ' + err, 'error');
                scanning = false;
                html5QrCode = null;
            });
        }

        function restartScanner() {
            if (!html5QrCode) return;
            scanning = false;
            if (retryTimer) { clearTimeout(retryTimer); retryTimer = null; }

            html5QrCode.stop().then(function() {
                // Apagar flash si estaba encendido
                if (flashOn) {
                    flashOn = false;
                    var btn = document.getElementById('flashBtn');
                    btn.textContent = '🔦 Flash';
                    btn.className = '';
                }
                // Reiniciar con constraints relajados en el 2do+ reintento
                var relaxed = retryCount >= 2;
                startScanner(relaxed);
            }).catch(function(err) {
                showStatus('❌ Error al reiniciar: ' + err, 'error');
                scanning = false;
            });
        }

        // Iniciar automáticamente al cargar (solo si cámara disponible)
        window.addEventListener('load', function() {
            if (cameraAvailable) {
                setTimeout(function() { startScanner(false); }, 500);
            }
        });
    </script>
</body>
</html>";
    }

    private static string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    var ipStr = ip.ToString();
                    // Saltar IPs locales de loopback y docker (172.17.x.x)
                    if (ipStr.StartsWith("127.") || ipStr.StartsWith("172.17."))
                        continue;
                    return ipStr;
                }
            }
        }
        catch { }

        return "localhost";
    }

    /// <summary>
    /// Obtiene todas las IPs locales para mostrar al usuario (por si tiene varias interfaces).
    /// </summary>
    public static List<string> GetLocalIpAddresses()
    {
        var ips = new List<string>();
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    var ipStr = ip.ToString();
                    if (!ipStr.StartsWith("127.") && !ipStr.StartsWith("172.17."))
                        ips.Add(ipStr);
                }
            }
        }
        catch { }

        if (ips.Count == 0) ips.Add("localhost");
        return ips;
    }

    private class ScanRequest
    {
        public string? Barcode { get; set; }
    }
}
