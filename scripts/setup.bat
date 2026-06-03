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

echo [1/3] Restaurando paquetes NuGet...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Fallo la restauracion de paquetes.
    pause
    exit /b 1
)
echo OK.
echo.

echo [2/3] Compilando solucion...
dotnet build SistemaGVP.sln --nologo
if %errorlevel% neq 0 (
    echo ERROR: Fallo la compilacion.
    pause
    exit /b 1
)
echo OK.
echo.

echo [3/3] Creando/actualizando base de datos...
dotnet ef database update --project src\SistemaGVP.Infrastructure --startup-project src\SistemaGVP.WPF
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
