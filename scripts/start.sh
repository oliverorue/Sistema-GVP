#!/bin/bash
# ========================================
#   Sistema GVP POS - Punto de Venta
#   NOTA: WPF solo funciona en Windows.
#   Este script solo es valido en Windows
#   con WSL/Git Bash.
# ========================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo "========================================"
echo "  Sistema GVP POS - Punto de Venta"
echo "========================================"
echo ""

if [[ "$(uname -s)" == MINGW* || "$(uname -s)" == CYGWIN* || "$(uname -s)" == MSYS* ]]; then
    echo "[1/2] Compilando proyecto WPF..."
    dotnet build "$PROJECT_DIR/src/SistemaGVP.WPF/SistemaGVP.WPF.csproj" --nologo
    echo ""

    echo "[2/2] Iniciando aplicacion..."
    dotnet run --project "$PROJECT_DIR/src/SistemaGVP.WPF/SistemaGVP.WPF.csproj" --no-build
else
    echo "ERROR: WPF solo funciona en Windows."
    echo "Este proyecto requiere .NET 8 SDK + Windows 10/11."
    echo ""
    echo "Ejecute scripts\\start.bat desde una terminal de Windows."
    exit 1
fi
