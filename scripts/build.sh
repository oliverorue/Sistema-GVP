#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

PUBLISH_DIR="$PROJECT_DIR/src/electron-app/backend-publish"
FRONTEND_DIR="$PROJECT_DIR/src/electron-app"
RELEASE_DIR="$FRONTEND_DIR/release"

# ─── Platform selection ─────────────────────────
PLATFORM="${1:-linux}"
if [ "$PLATFORM" != "linux" ] && [ "$PLATFORM" != "win" ] && [ "$PLATFORM" != "all" ]; then
  echo "Uso: $0 [linux|win|all]"
  echo "  linux  — AppImage (Linux)"
  echo "  win    — NSIS Installer (Windows)"
  echo "  all    — Ambos"
  exit 1
fi

DOTNET_RID="linux-x64"
BUILDER_FLAG="--linux"
if [ "$PLATFORM" = "win" ]; then DOTNET_RID="win-x64"; BUILDER_FLAG="--win"; fi

echo "========================================"
echo "  Sistema GVP POS — Build ($PLATFORM)"
echo "========================================"
echo ""

# ─── Clean ─────────────────────────────────────
echo "[1/5] Limpiando builds anteriores..."
rm -rf "$PUBLISH_DIR"
rm -rf "$FRONTEND_DIR/dist"
rm -rf "$FRONTEND_DIR/dist-electron"
if [ "$PLATFORM" != "all" ]; then rm -rf "$RELEASE_DIR"; fi
echo "  OK"
echo ""

# ─── Icon ──────────────────────────────────────
echo "[2/5] Verificando ícono..."
node "$SCRIPT_DIR/generate-icons.js"
echo ""

# ─── Backend ───────────────────────────────────
echo "[3/5] Publicando backend .NET ($DOTNET_RID)..."
dotnet publish "$PROJECT_DIR/src/SistemaGVP.API/SistemaGVP.API.csproj" \
  -c Release --self-contained -r "$DOTNET_RID" \
  -o "$PUBLISH_DIR" \
  -p:DebugType=none -p:DebugSymbols=false
echo "  OK"
echo ""

# ─── Electron ──────────────────────────────────
echo "[4/5] Compilando y empaquetando Electron..."
cd "$FRONTEND_DIR"
npx tsc -p tsconfig.node.json
npx vite build

if [ "$PLATFORM" = "all" ]; then
  npx electron-builder --linux --win
else
  npx electron-builder $BUILDER_FLAG
fi
echo ""

# ─── Result ────────────────────────────────────
echo "[5/5] Resultado:"
echo ""
if [ "$PLATFORM" = "linux" ] || [ "$PLATFORM" = "all" ]; then
  if ls "$RELEASE_DIR"/*.AppImage 2>/dev/null; then
    echo "  ✅ Linux:  $(ls "$RELEASE_DIR"/*.AppImage 2>/dev/null)"
  else
    echo "  ⚠️  Linux AppImage no encontrado"
  fi
fi
if [ "$PLATFORM" = "win" ] || [ "$PLATFORM" = "all" ]; then
  if ls "$RELEASE_DIR"/*-Setup.exe 2>/dev/null; then
    echo "  ✅ Windows: $(ls "$RELEASE_DIR"/*-Setup.exe 2>/dev/null)"
  else
    echo "  ⚠️  Windows Installer no encontrado"
  fi
fi

echo ""
echo "========================================"
echo "  Build completado."
echo "  Los instaladores están en: $RELEASE_DIR"
echo "========================================"

cd "$PROJECT_DIR"
