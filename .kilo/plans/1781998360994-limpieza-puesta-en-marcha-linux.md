# Plan: Limpieza y Puesta en Marcha — Sistema GVP POS (Linux)

## Resumen

El repo contiene 2.2 GB de los cuales ~1.6 GB son basura (builds Windows huérfanos,
artefactos de compilación viejos, archivos trackeados que ya no existen en disco).
La API (.NET 9 Minimal API) y el frontend (Electron + React + Vite) existen en
disco pero nunca se commitearon. El objetivo es limpiar, committear la versión
funcional, y dejarlo corriendo en Linux para desarrollo.

---

## Fase 1 — Limpieza del disco (libera ~1.6 GB)

### 1.1 Eliminar builds Windows de producción

| Ruta | Tamaño | Motivo |
|------|--------|--------|
| `src/electron-app/release/` | 1.4 GB | Instalador .exe, win-unpacked, win-test-* — generados por `build.bat` |
| `src/electron-app/backend-publish/` | 108 MB | `dotnet publish -r win-x64` — no funciona en Linux |

Acción:
```bash
rm -rf src/electron-app/release/
rm -rf src/electron-app/backend-publish/
```

### 1.2 Eliminar artefactos .NET 8 viejos

La API se compiló contra net8.0 antes de migrar a net9.0. Quedan obj/ híbridos.

| Ruta | Motivo |
|------|--------|
| `src/SistemaGVP.API/obj/Debug/net8.0/` | Build antiguo |
| `src/SistemaGVP.API/obj/Release/net8.0/` | Build antiguo (win-x64) |

Se regeneran solos con `dotnet build`. Acción:
```bash
rm -rf src/SistemaGVP.API/obj/Debug/net8.0/
rm -rf src/SistemaGVP.API/obj/Release/net8.0/
```

### 1.3 Eliminar builds previos del frontend

| Ruta | Motivo |
|------|--------|
| `src/electron-app/dist/` | Build Vite generado |
| `src/electron-app/dist-electron/` | Build Electron generado |

Se regeneran con `npm run build`. Acción:
```bash
rm -rf src/electron-app/dist/
rm -rf src/electron-app/dist-electron/
```

### 1.4 Eliminar archivos runtime huérfanos

| Archivo | Motivo |
|---------|--------|
| `api.err` | Cero bytes, leftover |
| `api.log` | Cero bytes, leftover |

Acción:
```bash
rm -f api.err api.log
```

### 1.5 Eliminar databases residuales en builds

```bash
find . -path "*/backend-publish/*.db*" -delete 2>/dev/null
find . -path "*/release/*.db*" -delete 2>/dev/null
```

### 1.6 Limpiar git de archivos eliminados del disco

El commit inicial trackea archivos que ya no existen:

- `src/SistemaGVP.WPF/` — proyecto WPF completo (~100+ archivos, eliminado del disco)
- `repomix-output.xml` — exportación XML, eliminada del disco

Acción:
```bash
git rm -r src/SistemaGVP.WPF/
git rm repomix-output.xml
```

### 1.7 Actualizar `.gitignore`

Garantizar que cubra todo el output generado:

```
# Ya existe:
src/electron-app/node_modules/
src/electron-app/dist/
src/electron-app/dist-electron/
src/electron-app/release/
src/electron-app/backend-publish/
api.err
api.log
scripts/private.pem

# Agregar (si no existen):
*.tsbuildinfo
```

Verificar que no haya excludes que cachan archivos fuente de API/Electron.

---

## Fase 2 — Puesta en funcionamiento

### 2.1 Commitear proyectos que faltan

`src/SistemaGVP.API/` y `src/electron-app/` están en disco pero no versionados.
Deben agregarse al repo (sin `node_modules/`, sin `dist/`, sin `obj/` — ya
ignorados por `.gitignore`).

```bash
git add src/SistemaGVP.API/
git add src/electron-app/ -- src/electron-app/package.json src/electron-app/package-lock.json src/electron-app/tsconfig*.json src/electron-app/vite.config.ts src/electron-app/index.html src/electron-app/postcss.config.mjs src/electron-app/tailwind.config.ts src/electron-app/electron/ src/electron-app/src/ src/electron-app/electron-builder.yml
```

También los nuevos scripts:
```bash
git add scripts/setup.sh scripts/start.sh scripts/build.sh scripts/generate-*.js
```

### 2.2 Verificar compilación .NET

```bash
dotnet build --nologo
dotnet test --nologo
# Debe compilar 5 proyectos y pasar 41 tests
```

### 2.3 Verificar frontend

```bash
cd src/electron-app
npm install   # regenera node_modules si se borró
npm run build # compila Vite + TypeScript
```

### 2.4 Verificar setup completo

```bash
./scripts/setup.sh
# Debe restaurar NuGet, compilar, npm install, migrar DB sin errores
```

### 2.5 Verificar modo desarrollo

```bash
./scripts/start.sh  → Opción 1
# API en http://127.0.0.1:5000, Frontend en http://localhost:5173
```

### 2.6 Commit final

```bash
git add -A
git commit -m "feat: migración a .NET 9, scripts Linux, limpieza de build artifacts Windows"
```

---

## Fase 3 — Mejoras futuras (fuera de este plan)

| Área | Problema | Solución propuesta |
|------|----------|--------------------|
| Impresión | `webContents.print()` requiere CUPS | Documentar configuración o abstraer con `node-printer` |
| Build producción Linux | No genera AppImage todavía | `scripts/build.sh` ya listo, probar `electron-builder --linux` |
| Scanner | Basado en keyboard events, puede fallar con ciertos lectores | Probar con hardware real, considerar `node-hid` si es necesario |
| Script `.bat` | Los .bat heredados ya no sirven en Linux | Mantenerlos por ahora (compatibilidad), no afectan |
| `electron-updater` | Configurado pero sin servidor de releases | Configurar o desactivar hasta tener backend de updates |
| Tests frontend | No hay tests del lado React | Agregar Vitest + React Testing Library |
| Seguridad | Secretos en `private.pem` están gitignoreados pero existen | Rotar claves RSA antes de primer commit público |

---

## Riesgos y validación

| Riesgo | Mitigación |
|--------|------------|
| `.gitignore` deja afuera algún archivo necesario de API/Electron | Hacer `git status` después de `git add` para verificar |
| `package-lock.json` desactualizado | `npm install` regenera, verificar que no haya breaking changes |
| `dotnet ef` falla si falta runtime | Ya migrado a net9.0, verificado que funciona |
| Migraciones de BD inconsistentes | `dotnet ef database update` ya ejecutado exitosamente |

## Criterio de éxito

- [x] `dotnet build` compila sin errores ni warnings
- [x] `dotnet test` pasa 41/41 tests
- [x] `dotnet ef database update` ejecuta sin errores
- [x] `npm install` completa sin errores
- [x] `git status` no muestra basura
- [x] Peso del proyecto < 500 MB (vs 2.2 GB actual)
