@echo off
:: ========================================
::   Sistema GVP POS - Punto de Venta
::   Configuracion inicial (una sola vez)
:: ========================================

title Sistema GVP - Setup
cd /d "%~dp0.."

echo ========================================
echo   Sistema GVP POS - Setup Inicial
echo ========================================
echo.

echo [1/4] Restaurando paquetes NuGet...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Fallo la restauracion de paquetes.
    pause
    exit /b 1
)
echo OK.
echo.

echo [2/4] Compilando solucion .NET...
dotnet build SistemaGVP.sln --nologo
if %errorlevel% neq 0 (
    echo ERROR: Fallo la compilacion.
    pause
    exit /b 1
)
echo OK.
echo.

echo [3/4] Instalando dependencias del frontend...
cd src\electron-app
call npm install
if %errorlevel% neq 0 (
    echo ERROR: Fallo la instalacion de dependencias.
    pause
    exit /b 1
)
cd /d "%~dp0.."
echo OK.
echo.

echo [3b/4] Instalando binario de Electron para Windows...
cd src\electron-app
node node_modules\electron\install.js 2>nul
cd /d "%~dp0.."
echo OK.
echo.

echo [4/4] Creando/actualizando base de datos...
dotnet ef database update --project src\SistemaGVP.Infrastructure --startup-project src\SistemaGVP.API
if %errorlevel% neq 0 (
    echo ERROR: Fallo la migracion de base de datos.
    pause
    exit /b 1
)
echo OK.
echo.

echo ========================================
echo   Setup completado exitosamente.
echo   Ejecute scripts\start.bat para iniciar.
echo ========================================
pause
