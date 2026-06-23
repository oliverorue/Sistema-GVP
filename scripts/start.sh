#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

API_BIN="$PROJECT_DIR/src/SistemaGVP.API/bin"
API_OBJ="$PROJECT_DIR/src/SistemaGVP.API/obj"
PUBLISH_DIR="$PROJECT_DIR/src/electron-app/backend-publish"
FRONTEND_DIR="$PROJECT_DIR/src/electron-app"

API_PID=""

cleanup() {
  if [ -n "$API_PID" ] && kill -0 "$API_PID" 2>/dev/null; then
    echo ""
    echo "Deteniendo API (PID $API_PID)..."
    kill "$API_PID" 2>/dev/null
    wait "$API_PID" 2>/dev/null
  fi
  exit 0
}
trap cleanup SIGINT SIGTERM EXIT

# ─── Helper: wait for API health ───────────────────
wait_for_api() {
  echo "Esperando a que la API responda..."
  for i in $(seq 1 30); do
    if curl -sf http://127.0.0.1:5000/health >/dev/null 2>&1; then
      echo "API lista."
      return 0
    fi
    sleep 1
  done
  echo "ERROR: La API no respondió después de 30 segundos."
  return 1
}

# ═══════════════════════════════════════════════════
echo "========================================"
echo "  Sistema GVP POS - Inicio (Linux)"
echo "========================================"
echo ""

# ─── Clean stale builds ──────────────────────────
echo "[1/4] Limpiando compilaciones anteriores..."
rm -rf "$API_BIN" "$API_OBJ" "$PUBLISH_DIR"
echo "  OK"
echo ""

# ─── Restore + Build backend ────────────────────
echo "[2/4] Restaurando y compilando backend..."
dotnet restore "$PROJECT_DIR" --nologo 2>&1 | tail -1
dotnet build "$PROJECT_DIR/SistemaGVP.sln" --nologo 2>&1 | tail -1
echo "  OK"
echo ""

# ─── Frontend dependencies ──────────────────────
echo "[3/4] Verificando dependencias del frontend..."
if [ ! -d "$FRONTEND_DIR/node_modules" ]; then
  cd "$FRONTEND_DIR"
  npm install --silent
  echo "  Dependencias instaladas"
else
  echo "  node_modules existe, omitiendo npm install"
fi
echo ""

# ─── Mode: always development (API + Vite) ──────
# Mode 1 is the default. Use scripts/build.sh for production builds.
echo "[4/4] Iniciando API en http://127.0.0.1:5000..."
dotnet run --project "$PROJECT_DIR/src/SistemaGVP.API/SistemaGVP.API.csproj" &
API_PID=$!

wait_for_api

echo ""
echo "Iniciando frontend React en http://localhost:5173..."
echo "  (Abrí esa URL en el navegador)"
echo ""
cd "$FRONTEND_DIR"
npm run dev
