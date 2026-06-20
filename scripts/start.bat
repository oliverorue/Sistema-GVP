@echo off
:: ========================================
::   Sistema GVP POS - Punto de Venta
::   Modo desarrollo: API + Frontend
:: ========================================

title Sistema GVP - Dev

cd /d "%~dp0.."
set ROOT_DIR=%CD%

:: Kill any leftover processes from previous runs
taskkill /f /im dotnet.exe 2>nul
taskkill /f /im node.exe 2>nul
taskkill /f /im SistemaGVP.API.exe 2>nul

echo ========================================
echo   Sistema GVP POS
echo ========================================
echo.
echo Elige el modo de inicio:
echo   [1] API + React Vite  - Backend + Frontend en navegador (desarrollo)
echo   [2] API sola          - Solo backend .NET en http://127.0.0.1:5000
echo   [3] Produccion local  - Backend compilado + Electron (requiere build.bat primero)
echo.
echo.

set /p MODE="Selecciona (1/2/3): "

if "%MODE%"=="1" (
    echo.
    echo [1/2] Iniciando API en http://127.0.0.1:5000...
    start "SistemaGVP API" cmd /c "cd /d %ROOT_DIR% && dotnet run --project src\SistemaGVP.API\SistemaGVP.API.csproj"
    
    :: Wait for API health check (use PowerShell for compatibility)
    echo Esperando a que la API responda...
    for /l %%i in (1,1,30) do (
        powershell -NoProfile -Command "try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:5000/health' -UseBasicParsing; if ($r.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }" >nul 2>&1
        if not errorlevel 1 goto :api_ready
        >nul 2>&1 timeout /t 1 /nobreak
    )
    echo ERROR: La API no respondio despues de 30 segundos.
    echo Revisa la ventana 'SistemaGVP API' para ver errores.
    pause
    exit /b 1
    :api_ready
    echo API lista.
    echo.
    
    echo [2/2] Iniciando frontend React...
    cd /d "%ROOT_DIR%\src\electron-app"
    start "SistemaGVP React" cmd /c "npm run dev"
    echo.
    echo Abre http://localhost:5173 en tu navegador
    timeout /t 3 >nul
    start http://localhost:5173
    goto :eof
)

if "%MODE%"=="2" (
    echo.
    echo Iniciando API sola en http://127.0.0.1:5000...
    dotnet run --project "src\SistemaGVP.API\SistemaGVP.API.csproj"
    goto :eof
)

if "%MODE%"=="3" (
    echo.
    echo [1/2] Iniciando backend compilado...
    start "SistemaGVP API" cmd /c "cd /d "%ROOT_DIR%\src\electron-app\backend-publish" && SistemaGVP.API.exe --urls http://127.0.0.1:5000"

    echo Esperando a que la API responda...
    for /l %%i in (1,1,30) do (
        powershell -NoProfile -Command "try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:5000/health' -UseBasicParsing; if ($r.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }" >nul 2>&1
        if not errorlevel 1 goto :prod_api_ready
        >nul 2>&1 timeout /t 1 /nobreak
    )
    echo ERROR: La API no respondio despues de 30 segundos.
    pause
    exit /b 1
    :prod_api_ready
    echo API lista.
    echo.

    echo [2/2] Iniciando Electron...
    set ELECTRON_EXE=%ROOT_DIR%\src\electron-app\release\win-unpacked\Sistema GVP.exe
    if exist "%ELECTRON_EXE%" (
        echo Usando app empaquetada: win-unpacked\Sistema GVP.exe
        start "Sistema GVP" cmd /c "set GVPSKIPBACKEND=1 && "%ELECTRON_EXE%""
    ) else (
        echo Modo desarrollo: npx electron .
        cd /d "%ROOT_DIR%\src\electron-app"
        start "Sistema GVP" cmd /c "set GVPSKIPBACKEND=1 && npx electron ."
    )
    echo.
    echo Aplicacion Electron iniciada.
    cd /d "%ROOT_DIR%"
    goto :eof
)

echo Opcion no valida.
pause
