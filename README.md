# Sistema GVP POS — Electron + React + .NET 9

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![Electron](https://img.shields.io/badge/Electron-31-47848F?logo=electron)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-yellow)

**Sistema GVP** — Punto de venta para ferreterías. Desktop app offline-first con Electron + React + .NET 9 + SQLite.

---

## Inicio Rápido

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- Windows 10/11 o Linux

### Primera configuración (solo una vez)

**Windows:**
```
scripts\setup.bat
```

**Linux:**
```bash
./scripts/setup.sh
```

Esto restaura paquetes NuGet, compila la solución, instala dependencias del frontend y crea la base de datos SQLite.

### Build producción

**Windows:**
```
scripts\build.bat
```

**Linux:**
```bash
./scripts/build.sh
```

Esto compila el backend .NET como standalone (linux-x64 en Linux, win-x64 en Windows) y empaqueta la aplicación con electron-builder. El instalador se genera en `src/electron-app/release/`.

---

## Características

### Gestión
- Productos, Categorías, Clientes y Proveedores con CRUD completo
- Control de stock con alertas de stock bajo
- Unidades fraccionarias (kg, m, litro) para ferretería
- Código de barras y SKU

### Ventas
- Búsqueda de productos por nombre o código de barras
- Carrito con cantidades fraccionarias
- Múltiples métodos de pago (Efectivo/Tarjeta/Transferencia)
- **Ventas en Espera**: pausar y reanudar ventas pendientes (F8)
- **Selección de cliente** con validación de límite de crédito
- **IVA dinámico** desde configuración de empresa
- **Impresión de tickets** (Electron) — automática al cobrar, reimpresión desde historial
- Atajos de teclado: F1 (buscar), F2 (cliente), F5 (cobrar), F8 (pausar), ESC (cerrar)

### Reportes
- Ventas por período, stock bajo, margen de ganancia, valorización de inventario
- Tablas con datos y gráficos (BarChart, AreaChart)
- Exportación a Excel y PDF

### Administración
- Usuarios con roles (Admin/Cajero) y reseteo de contraseña
- Configuración de empresa (nombre, RUC, impuesto, moneda, umbral de stock)
- Backup y restauración con verificación SHA256
- Auditoría completa con filtros

### Licencias
- Sistema de activación offline con RSA 2048
- Período de prueba de 30 días
- Generación de claves mediante scripts CLI

---

## Estructura del Proyecto

```
src/
├── SistemaGVP.API/             # Minimal API .NET 9 (56 endpoints)
├── SistemaGVP.Domain/          # Entidades y Enums
├── SistemaGVP.Application/     # DTOs, Servicios, Interfaces
├── SistemaGVP.Infrastructure/  # EF Core, Repositorios, SQLite
└── electron-app/               # Frontend Electron + React + TypeScript
    ├── electron/               # Main process (backend manager, printer, license)
    ├── src/
    │   ├── components/         # UI components (ui/, data-table/, shared/, charts/)
    │   ├── hooks/              # Custom hooks (useApi, useAuth, usePrintTicket...)
    │   ├── screens/            # 15 screens
    │   ├── services/           # API services (axios)
    │   ├── stores/             # Zustand stores (auth, cart, UI)
    │   ├── types/              # TypeScript interfaces
    │   └── utils/              # Formatters, constants
    └── ...
scripts/                        # build.bat, setup.bat, generate-keys.js
tests/                          # .NET unit tests
```

---

## Tecnologías

| Frontend | Backend |
|----------|---------|
| React 19 + TypeScript | .NET 9 Minimal API |
| Vite 6 | Entity Framework Core |
| Tailwind CSS 3 | SQLite |
| Zustand (estado) | AutoMapper |
| react-hook-form + zod | BCrypt |
| @tanstack/react-table | Serilog |
| Recharts | FluentValidation |
| sonner (toasts) | |
| lucide-react (iconos) | |
| Electron 31 | |

---

## Atajos de Teclado (Pantalla de Ventas)

| Tecla | Acción |
|-------|--------|
| F1 | Foco en búsqueda de productos |
| F2 | Abrir selección de cliente |
| F5 | Abrir diálogo de cobro |
| F8 | Pausar venta |
| ESC | Cerrar modal activo |

---

## Licencias

1. `node scripts\generate-keys.js` — genera par RSA 2048
2. Copiar PUBLIC_KEY en `electron-app/electron/license.ts`
3. `node scripts\generate-license.js --company="Cliente" --machineId="ID"` — genera clave

---

## Credenciales por Defecto

| Usuario | Contraseña | Rol |
|---------|-----------|------|
| admin | admin123 | Administrador |
| cajero | cajero123 | Cajero |

---

## Solución de Problemas

**Error: "No se encuentra dotnet"** → Instalar .NET 9 SDK

**Error de base de datos** → Eliminar `sistemagvp.db` y ejecutar `scripts\setup.bat`

**La app Electron no abre** → Verificar que `scripts\build.bat` se ejecutó primero (modo producción)

**Impresión no funciona** → Configurar impresora predeterminada en Windows

---

## Licencia

Este proyecto está licenciado bajo la licencia MIT. Consulte el archivo [LICENSE](LICENSE) para más detalles.
