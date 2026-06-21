#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

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

echo "========================================"
echo "  Sistema GVP POS - Inicio (Linux)"
echo "========================================"
echo ""
echo "Elige el modo de inicio:"
echo "  [1] API + React Vite  - Backend + Frontend en navegador (desarrollo)"
echo "  [2] API sola          - Solo backend en http://127.0.0.1:5000"
echo "  [3] Produccion local  - Backend compilado + Electron (requiere build.sh primero)"
echo ""

read -p "Selecciona (1/2/3): " MODE

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

case "$MODE" in
  1)
    echo ""
    echo "[1/2] Iniciando API en http://127.0.0.1:5000..."
    dotnet run --project "$PROJECT_DIR/src/SistemaGVP.API/SistemaGVP.API.csproj" &
    API_PID=$!

    wait_for_api

    echo ""
    echo "[2/2] Iniciando frontend React..."
    cd "$PROJECT_DIR/src/electron-app"
    npm run dev
    ;;

  2)
    echo ""
    echo "Iniciando API sola en http://127.0.0.1:5000..."
    dotnet run --project "$PROJECT_DIR/src/SistemaGVP.API/SistemaGVP.API.csproj"
    ;;

  3)
    echo ""
    BACKEND_PUBLISH_DIR="$PROJECT_DIR/src/electron-app/backend-publish"
    BACKEND_BIN="$BACKEND_PUBLISH_DIR/SistemaGVP.API"

    if [ ! -f "$BACKEND_BIN" ]; then
      echo "Backend compilado no encontrado. Publicando..."
      dotnet publish "$PROJECT_DIR/src/SistemaGVP.API/SistemaGVP.API.csproj" \
        -c Release --self-contained -r linux-x64 \
        -o "$BACKEND_PUBLISH_DIR"
      echo "Publicación completada."
    fi

    echo "[1/2] Iniciando backend compilado..."
    cd "$BACKEND_PUBLISH_DIR"
    ./SistemaGVP.API --urls http://127.0.0.1:5000 &
    API_PID=$!
    cd "$PROJECT_DIR"

    wait_for_api

    echo ""
    echo "[2/2] Iniciando Electron..."
    cd "$PROJECT_DIR/src/electron-app"
    GVPSKIPBACKEND=1 npx electron .
    ;;

  *)
    echo "Opción no válida."
    exit 1
    ;;
esac
