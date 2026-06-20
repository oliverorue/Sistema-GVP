#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo "========================================"
echo "  Sistema GVP POS - Setup Inicial (Linux)"
echo "========================================"
echo ""

echo "[1/4] Restaurando paquetes NuGet..."
dotnet restore "$PROJECT_DIR"
echo "OK."
echo ""

echo "[2/4] Compilando solución .NET..."
dotnet build "$PROJECT_DIR/SistemaGVP.sln" --nologo
echo "OK."
echo ""

echo "[3/4] Instalando dependencias del frontend..."
cd "$PROJECT_DIR/src/electron-app"
npm install
echo "OK."
echo ""

echo "[4/4] Creando/actualizando base de datos..."
cd "$PROJECT_DIR"
dotnet ef database update --project src/SistemaGVP.Infrastructure --startup-project src/SistemaGVP.API
echo "OK."
echo ""

echo "========================================"
echo "  Setup completado exitosamente."
echo "  Ejecute: scripts/start.sh"
echo "========================================"
