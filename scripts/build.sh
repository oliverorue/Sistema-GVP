#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo "========================================"
echo "  Sistema GVP POS - Build Produccion (Linux)"
echo "========================================"
echo ""

echo "[1/3] Generando iconos placeholder..."
node "$SCRIPT_DIR/generate-icons.js"
echo "OK."
echo ""

echo "[2/3] Publicando backend .NET (linux-x64 standalone)..."
dotnet publish "$PROJECT_DIR/src/SistemaGVP.API/SistemaGVP.API.csproj" \
  -c Release --self-contained -r linux-x64 \
  -o "$PROJECT_DIR/src/electron-app/backend-publish"
echo "OK."
echo ""

echo "[3/3] Compilando frontend Electron + empaquetando..."
cd "$PROJECT_DIR/src/electron-app"
npm run electron:build -- --linux
echo "OK."
echo ""

echo "========================================"
echo "  Build completado exitosamente."
echo "  El instalador/AppImage se encuentra en:"
echo "  src/electron-app/release/"
echo "========================================"
