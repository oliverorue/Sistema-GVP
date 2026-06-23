@echo off
title Sistema GVP POS
cd /d "%~dp0.."

echo ========================================
echo   Sistema GVP POS - Inicio
echo ========================================
echo.

set "BACKEND_DIR=%~dp0..\src\electron-app\backend-publish"
set "APP_DIR=%~dp0..\src\electron-app"

if not exist "%BACKEND_DIR%\SistemaGVP.API.exe" (
    echo [1/3] Publicando backend .NET...
    call dotnet publish src\SistemaGVP.API\SistemaGVP.API.csproj -c Release --self-contained -r win-x64 -o "%BACKEND_DIR%" -p:DebugType=none -p:DebugSymbols=false
    if errorlevel 1 (
        echo ERROR: Fallo la publicacion del backend
        pause
        exit /b 1
    )
)

echo [1/3] Iniciando backend API en http://127.0.0.1:5000...
set "ASPNETCORE_CONTENTROOT=%BACKEND_DIR%"
start /b "" "%BACKEND_DIR%\SistemaGVP.API.exe" --urls http://127.0.0.1:5000

echo       Esperando que el backend este listo...
:wait
timeout /t 2 /nobreak >nul
>nul 2>&1 curl -s http://127.0.0.1:5000/health && (
    echo       Backend listo!
    goto ready
)
echo       . 
goto wait

:ready
echo.
echo [2/3] Iniciando aplicacion Electron...
set "GVPSKIPBACKEND=1"
start "" "%APP_DIR%\node_modules\.bin\electron.cmd" "%APP_DIR%" --no-sandbox

echo [3/3] Hecho. La aplicacion se abrira en unos segundos.
echo.
echo Para cerrar: cierre la ventana de Electron y ejecute:
echo   taskkill /f /im SistemaGVP.API.exe
echo.
echo NOTA: Para modo desarrollo, use directamente scripts\start.bat
echo       Para produccion, instale el setup y use scripts\start-prod.bat
