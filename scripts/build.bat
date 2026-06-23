@echo off
title Sistema GVP - Build Produccion
cd /d "%~dp0.."

set PUBLISH_DIR=%CD%\src\electron-app\backend-publish
set FRONTEND_DIR=%CD%\src\electron-app
set RELEASE_DIR=%FRONTEND_DIR%\release

echo ========================================
echo   Sistema GVP POS — Build (win)
echo ========================================
echo.

:: ─── Clean ─────────────────────────────────────
echo [1/5] Limpiando builds anteriores...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%FRONTEND_DIR%\dist" rmdir /s /q "%FRONTEND_DIR%\dist"
if exist "%FRONTEND_DIR%\dist-electron" rmdir /s /q "%FRONTEND_DIR%\dist-electron"
if exist "%RELEASE_DIR%" rmdir /s /q "%RELEASE_DIR%"
echo   OK
echo.

:: ─── Icon ──────────────────────────────────────
echo [2/5] Verificando icono...
node scripts\generate-icons.js
if %errorlevel% neq 0 (
    echo ERROR: Fallo la generacion de iconos.
    pause
    exit /b 1
)
echo   OK
echo.

:: ─── Backend ───────────────────────────────────
echo [3/5] Publicando backend .NET (win-x64)...
dotnet publish src\SistemaGVP.API\SistemaGVP.API.csproj ^
  -c Release --self-contained -r win-x64 ^
  -o "%PUBLISH_DIR%" ^
  -p:DebugType=none -p:DebugSymbols=false
if %errorlevel% neq 0 (
    echo ERROR: Fallo la publicacion del backend.
    pause
    exit /b 1
)
echo   OK
echo.

:: ─── Electron ──────────────────────────────────
echo [4/5] Compilando y empaquetando Electron...
cd /d "%FRONTEND_DIR%"
call npx tsc -p tsconfig.node.json
if %errorlevel% neq 0 (
    echo ERROR: Fallo la compilacion TypeScript.
    pause
    exit /b 1
)
call npx vite build
if %errorlevel% neq 0 (
    echo ERROR: Fallo el build de Vite.
    pause
    exit /b 1
)
call npx electron-builder --win
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
cd /d "%CD%"
echo.

:: ─── Result ────────────────────────────────────
echo [5/5] Resultado:
echo.
if exist "%RELEASE_DIR%\*-Setup.exe" (
    for %%f in ("%RELEASE_DIR%\*-Setup.exe") do echo   Windows: %%f
) else (
    echo   Windows Installer no encontrado
)
echo.
echo ========================================
echo   Build completado.
echo   Los instaladores estan en: %RELEASE_DIR%
echo ========================================
pause
