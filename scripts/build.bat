@echo off
title Sistema GVP - Build Produccion
cd /d "%~dp0.."

echo ========================================
echo   Sistema GVP POS - Build Produccion
echo ========================================
echo.

echo [1/3] Generando iconos placeholder...
node scripts\generate-icons.js
if %errorlevel% neq 0 (
    echo ERROR: Fallo la generacion de iconos.
    pause
    exit /b 1
)
echo OK.
echo.

echo [2/3] Publicando backend .NET (win-x64 standalone)...
dotnet publish src\SistemaGVP.API\SistemaGVP.API.csproj -c Release --self-contained -r win-x64 -o src\electron-app\backend-publish
if %errorlevel% neq 0 (
    echo ERROR: Fallo la publicacion del backend.
    pause
    exit /b 1
)
echo OK.
echo.

echo [3/3] Compilando frontend Electron + empaquetando instalador...
cd src\electron-app

:: Pre-extract winCodeSign cache with -snl to avoid symlink errors
echo Verificando cache de empaquetado...
where 7za.exe >nul 2>&1
if %errorlevel% equ 0 (
    7za x "%TEMP%\wincodesign.7z" -o"%LOCALAPPDATA%\electron-builder\Cache\winCodeSign\2.6.0" -snl -bd -y >nul 2>&1
)

:: Set environment variables to disable code signing
set CSC_IDENTITY_AUTO_DISCOVERY=false

call npm run electron:build
if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo   ERROR: Fallo el empaquetado Electron.
    echo ========================================
    echo.
    echo Causa posible: falta de permisos para crear enlaces simbolicos en Windows.
    echo.
    echo Solucion 1: Ejecute este script como ADMINISTRADOR.
    echo   Haga clic derecho en "build.bat" ^> "Ejecutar como administrador"
    echo.
    echo Solucion 2: Active el "Modo Desarrollador" en Windows:
    echo   Configuracion ^> Actualizacion y seguridad ^> Para desarrolladores
    echo   ^> Activar "Modo desarrollador"
    echo.
    echo Solucion 3: Genere solo el compilado sin instalador:
    echo   cd src\electron-app
    echo   npm run build
    echo   npx electron-builder --win --dir
    echo.
    pause
    exit /b 1
)

cd ..\..
echo OK.
echo.

echo ========================================
echo   Build completado exitosamente.
echo   Instalador: src\electron-app\release\Sistema GVP Setup *.exe
echo ========================================
pause
