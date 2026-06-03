@echo off
:: ========================================
::   Sistema GVP POS - Punto de Venta
::   Windows Version (WPF .NET 8)
::   Un solo script: compila e inicia
:: ========================================

title Sistema GVP POS
cd /d "%~dp0.."

echo ========================================
echo   Sistema GVP POS - Punto de Venta
echo   (WPF .NET 8 - modo ventana nativa)
echo ========================================
echo.

:: --------------------------------------------------
:: 1. Verificar .NET SDK
:: --------------------------------------------------
echo [1/3] Verificando .NET SDK...
echo.

dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: No se detecto .NET SDK instalado.
    echo.
    echo Descargue e instale .NET 8 SDK desde:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

for /f "tokens=1" %%a in ('dotnet --version') do set DOTNET_VERSION=%%a
echo OK - .NET SDK %DOTNET_VERSION% detectado.
echo.

:: --------------------------------------------------
:: 2. Compilar solo el proyecto WPF (arrastra
::    Domain, Application, Infrastructure)
:: --------------------------------------------------
echo [2/3] Compilando proyecto WPF...
echo.

dotnet build "src\SistemaGVP.WPF\SistemaGVP.WPF.csproj" --nologo
if %errorlevel% neq 0 (
    echo ERROR: Fallo la compilacion. Revise los errores arriba.
    pause
    exit /b 1
)
echo Compilacion correcta.
echo.

:: --------------------------------------------------
:: 3. Iniciar la aplicacion WPF
:: --------------------------------------------------
echo [3/3] Iniciando Sistema GVP POS...
echo.

dotnet run --project "src\SistemaGVP.WPF\SistemaGVP.WPF.csproj" --no-build
if %errorlevel% neq 0 (
    echo.
    echo ERROR: La aplicacion termino con un error (codigo: %errorlevel%).
    pause
    exit /b 1
)
