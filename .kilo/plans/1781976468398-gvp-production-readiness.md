# Plan: Sistema GVP — Producción para Ferretería

## Contexto

El sistema ya tiene backend .NET completo (API con 56 endpoints), Electron shell funcional, 14 pantallas React y servicios HTTP. Pero las pantallas de gestión solo muestran datos — los botones Crear/Editar/Eliminar no tienen `onClick`. Falta implementar modales CRUD, pausar/reanudar ventas, impresión, reportes visuales, build de producción y licencias.

Se suman 6 refinamientos específicos para ferretería: cantidades fraccionarias (kilos/metros), IVA dinámico desde configuración, DataTable reusable con sorting, reimpresión de tickets, clientes con cuenta corriente, y pantalla de cambio de contraseña.

## Decisiones de Diseño Confirmadas

| Decisión | Elección |
|----------|----------|
| Componentes UI | Tailwind puro + react-hook-form + zod. Sin librería externa de componentes |
| Notificaciones (toasts) | `sonner` (~3KB, sin deps extra) |
| Sistema de licencias | RSA offline con generador de archivos `.lic` |
| Impresión | Navegador (webContents.print con HTML estilizado) |
| Tablas | `@tanstack/react-table` (ya instalado) para DataTable reutilizable con sorting por columna |
| Cantidades | `number` con decimales en quantity, display con unidad (`product.unit`) en carrito y ticket |
| IVA | Dinámico desde `GET /api/settings/company.taxRate`, no hardcodeado |

## Dependencias a Instalar

```bash
npm install sonner
# react-hook-form, @hookform/resolvers, zod, recharts, @tanstack/react-table ya están instalados
```

---

## Fase 1 — Componentes UI Reutilizables

**Objetivo:** Poblar los 6 directorios vacíos con componentes base que usarán todas las pantallas.

### 1.1 `components/ui/index.ts`
Barrel file que re-exporta todos los componentes de ui/.

### 1.2 `components/ui/Modal.tsx`
- Props: `isOpen`, `onClose`, `title`, `children`, `size?: 'sm' | 'md' | 'lg'`
- Fondo semi-transparente, centrado, animación fade-in
- Botón X para cerrar, click fuera del modal cierra
- Usa `createPortal` para montar en `document.body`

### 1.3 `components/ui/ConfirmDialog.tsx`
- Props: `isOpen`, `onClose`, `onConfirm`, `title`, `message`, `confirmLabel?`, `variant?: 'danger' | 'warning'`
- Modal con dos botones: Cancelar + Confirmar
- Variantes de color (danger = rojo, warning = ámbar)

### 1.4 `components/ui/FormField.tsx`
- Props: `label`, `error?`, `children`
- Envuelve inputs con label estilizado y mensaje de error condicional (texto rojo)

### 1.5 `components/shared/SearchInput.tsx`
- Props: `value`, `onChange`, `placeholder?`, `debounceMs?` (default 300)
- Input con ícono Search de lucide-react + debounce interno

### 1.6 `components/feedback/Toast.tsx`
- Instalar `sonner` y configurar `<Toaster />` en `AppLayout.tsx`
- Componente wrapper simple. Las pantallas usarán `toast.success()`, `toast.error()` directamente de sonner.

### 1.7 `src/hooks/useApi.ts`
- Hook que devuelve `{ get, post, put, delete }` wrappeados con try/catch y llamadas a toast.error automáticas
- Reemplaza los `fetch()` directos que hay en múltiples pantallas
- Usa el `api.ts` (axios) existente con interceptors

### 1.8 `components/data-table/DataTable.tsx`
- Usa `@tanstack/react-table` (ya instalado). Reutilizable en todas las pantallas.
- Props genéricas: `columns: ColumnDef<T>[]`, `data: T[]`, `loading?`, `emptyMessage?`, `page?`, `pageSize?`, `totalPages?`, `onPageChange?`, `onSearch?`
- Sorting por columna (click en header → ícono ▲/▼), estado vacío, estado loading (skeleton/spinner), paginación inferior con "Anterior / Página X de Y / Siguiente"
- Reemplazar las `<table>` HTML manuales en todas las pantallas de Fase 2

---

## Fase 2 — CRUD en Pantallas de Gestión

**Objetivo:** Cada pantalla debe permitir crear, editar y eliminar mediante modales con formularios validados.

**Patrón por pantalla:**
1. Estado `showModal` (tipo `'create' | 'edit' | null`) y `editingId`
2. Modal con formulario (react-hook-form + zod schema)
3. `onSubmit` llama al service, cierra modal, refresca lista, muestra toast
4. Delete usa `ConfirmDialog` → llama al service → refresca → toast
5. La tabla usa `DataTable` (1.8) en vez de `<table>` manual

### 2.1 ProductsScreen — Modal Create/Edit
Campos: `name`, `barcode`, `sku`, `categoryId` (select cargado de API), `supplierId` (select opcional), `price`, `cost`, `minStock`, `unit` (select: unidad/kg/m/litro), `description` (opcional)
- Validación zod: name requerido, price > 0, cost >= 0, barcode requerido, minStock >= 0
- Delete con ConfirmDialog

### 2.2 CategoriesScreen — Modal Create/Edit
Campos: `name`, `description` (opcional)
- Validación zod: name requerido, min 3 caracteres
- Delete con ConfirmDialog

### 2.3 CustomersScreen — Modal Create/Edit
Campos: `name`, `taxId` (opcional), `phone` (opcional), `email` (opcional), `address` (opcional), `creditLimit`
- Validación zod: name requerido
- Delete con ConfirmDialog

### 2.4 SuppliersScreen — Modal Create/Edit
Campos: `name`, `contactName`, `phone`, `email`, `address`, `taxId`
- Validación zod: name requerido

### 2.5 UsersScreen — Modal Create/Edit
Campos: `username`, `fullName`, `email`, `role` (select Admin/Cashier), `password` (solo create, min 6), `isActive`
- Validación zod: username y fullName requeridos, password requerido en create (min 6)
- Botón Reset Password: modal con campo `newPassword` → `POST /api/users/{id}/reset-password`

### 2.6 InventoryScreen — Modal Nuevo Movimiento
Campos: `productId` (search select con SearchInput), `type` (select Entry/Exit/Adjustment), `quantity` (acepta decimales), `reason`, `notes` (opcional)
- Validación zod: productId > 0, quantity != 0, reason requerido
- Si type es Entry → quantity > 0, si es Exit → quantity < 0 (o se ajusta automáticamente)

### 2.7 Conectar botones Edit/Delete con onClick
Revisar y conectar los `onClick` en Products, Categories, Customers, Suppliers, Users, Inventory, Backup.

### 2.8 Reemplazar fetch() directo por useApi hook
Pantallas que usan fetch en vez de api.ts: DashboardScreen, CategoriesScreen, CustomersScreen, SuppliersScreen, UsersScreen, SalesScreen (búsqueda)

### 2.9 `screens/ChangePasswordScreen.tsx`
- Pantalla accesible en la ruta `/change-password` (fuera del AppLayout, sin sidebar)
- Formulario: contraseña actual, contraseña nueva, confirmar nueva
- `POST /api/auth/change-password` → toast → redirigir a `/dashboard`
- Agregar ruta en `App.tsx` (ruta pública, no requiere ProtectedRoute pero sí token)

---

## Fase 3 — Funcionalidades Core de Ventas (con Refinamientos Ferretería)

### 3.1 IVA dinámico desde configuración
**Problema:** `cartStore.ts:36` hardcodea `TAX_RATE = 0.10`. Si la ferretería cambia el IVA, el carrito sigue calculando 10%.
**Solución:**
- Agregar campo `taxRate` al `CartState`, con default `0.10`
- Agregar acción `setTaxRate(rate: number)`
- `AppLayout.tsx` o `DashboardScreen.tsx`: al montar, hacer `GET /api/settings/company` y llamar `cartStore.setTaxRate(company.taxRate)`
- El cálculo de `tax()` usa `get().taxRate` en vez de constante

### 3.2 Cantidades fraccionarias (kilos, metros, litros)
**Problema:** El carrito y la UI tratan `quantity` como entero. Una ferretería vende 0.5 kg de clavos, 2.5 m de cable.
**Cambios:**
- `CartItem.quantity`: ya es `number`, solo asegurar que `updateQuantity` y el input acepten decimales (step="0.01" o step="0.1" según unidad)
- `SalesScreen`: en el carrito, mostrar unidad junto a cantidad: `0.5 kg × Gs. 8.000` en vez de solo `1 × Gs. 8.000`
- `SearchResult` en SalesScreen: incluir `unit` del producto
- `TicketTemplate` (Fase 5): mostrar `2.5 m` en vez de solo `2.5`
- `InventoryScreen`: quantity input acepta decimales

### 3.3 Pausar venta
- Conectar `onClick` del botón "Pausar" en SalesScreen → `saleService.holdSale(items)` → toast "Venta pausada" → limpiar carrito

### 3.4 Diálogo de Ventas en Espera
- Nuevo modal en `SalesScreen` que lista ventas pausadas (`GET /api/sales/held`)
- Cada venta muestra: total, fecha, cantidad de items, botón "Reanudar"
- "Reanudar" → `saleService.resumeSale(id)` → carga items en el carrito → toast

### 3.5 Selección de cliente en venta (con cuenta corriente)
- Botón "👤 Cliente" en el panel del carrito de `SalesScreen`
- Modal con SearchInput + lista de clientes (`GET /api/customers/search`)
- Cada cliente muestra: nombre, documento, **saldo actual** (`customer.balance`), **límite de crédito** (`customer.creditLimit`)
- Al seleccionar cliente: `cartStore.setCustomer(id, name)`
- **Validación cuenta corriente:** si `cart.total() + customer.balance > customer.creditLimit`, mostrar `ConfirmDialog` con advertencia: "El cliente superará su límite de crédito. Saldo actual: Gs. {balance}. ¿Continuar?"
- Si el cliente no tiene crédito (`creditLimit === 0`), solo ventas al contado (no aplicar advertencia)

### 3.6 Anular venta desde Historial
- `SalesHistoryScreen`: agregar botón "Anular" (visible solo si `useAuth().isAdmin`) en cada fila con estado "Completed"
- `ConfirmDialog` con campo `reason` → `saleService.voidSale(id, reason)` → refrescar → toast

### 3.7 Atajos de teclado
- `hooks/useKeyboardShortcuts.ts`: hook que registra handlers globales con `useEffect` + `keydown`
- F1: focus SearchInput en SalesScreen
- F2: abrir modal selección cliente
- F5: abrir diálogo de pago
- F8: pausar venta
- ESC: cerrar modales abiertos

---

## Fase 4 — Reportes y Dashboard Visual

### 4.1 `components/charts/ChartCard.tsx`
- Props: `title`, `children` (Recharts component)
- Card con título y área de gráfico responsive

### 4.2 Dashboard con gráficos
- Reemplazar sección de "Productos más vendidos" por `BarChart` de Recharts (productName vs totalQuantity)
- Reemplazar "Movimientos Recientes" por un resumen con `AreaChart` de ventas diarias (últimos 7 días) — requiere que el endpoint `/api/dashboard/summary` incluya datos de `dailySales` o se cree uno nuevo. Si no existe, usar `GET /api/reports/sales` con rango de 7 días.
- Usar `ChartCard` como wrapper

### 4.3 ReportsScreen: renderizar tablas y gráficos
- Reemplazar `<pre>{JSON.stringify(data)}</pre>` por:
  - **Ventas por período:** tabla con columnas (día, cantidad ventas, total, efectivo, tarjeta, transferencia) + `BarChart`
  - **Stock bajo:** tabla de productos con nombre, stock actual, stock mínimo, diferencia
  - **Margen de ganancia:** KPIs numéricos (costo total, venta total, margen, porcentaje) + tabla detalle
  - **Valorización de inventario:** tabla con producto, costo unitario, stock, valor total (costo × stock)

### 4.4 Reportes: filtros visuales
- Select de tipo de reporte
- DatePicker nativo HTML `type="date"` para desde/hasta
- Botón "Generar" con loading state
- Botones exportar Excel/PDF existentes (ya implementados)

---

## Fase 5 — Impresión de Tickets (Navegador)

### 5.1 Componente `components/shared/TicketTemplate.tsx`
- Props: `sale: Sale` (con detalles), `company: Company`
- Genera HTML estilizado para ticket 80mm (formato POS):
  - Header centrado: nombre empresa, RUC, dirección, teléfono
  - Línea separadora (`---`)
  - Fecha/hora, número de factura, cajero, cliente (si aplica)
  - Línea separadora
  - Items: `cantidad × unidad  nombre  subtotal` (ej: `2.5 m × Cable  Gs. 12.500`)
  - Línea separadora
  - Footer: Subtotal, IVA (%), Descuento, **TOTAL** (negrita)
  - Método de pago, efectivo recibido, cambio
  - Línea separadora
  - Mensaje centrado: "Gracias por su compra"
  - Pie: "Sistema GVP v2.0"

### 5.2 Hook `hooks/usePrintTicket.ts`
- `printSaleTicket(sale: Sale)`: renderiza `TicketTemplate` en ventana oculta, llama a `ipcRenderer.invoke('printer:print', html)` y luego cierra la ventana
- `printCurrentSale()`: wrapper que obtiene la sale recién creada y la imprime

### 5.3 Flujo completo de cobro
- `SalesScreen.handleCheckout` → success:
  1. Toast "Venta completada — Gs. {total}"
  2. `usePrintTicket.printCurrentSale()` → imprime ticket automáticamente
  3. `cart.clearCart()`
  4. `navigate('/sales-history')` (opcional, puede preferirse quedarse en SalesScreen para siguiente venta)

### 5.4 Reimpresión desde Historial
- `SalesHistoryScreen`: agregar botón 🖨️ en cada fila
- Al click: `GET /api/sales/{id}` → cargar datos completos → `usePrintTicket.printSaleTicket(sale)` → toast "Reimprimiendo ticket..."
- Útil cuando el papel se atasca o el cliente pide copia

---

## Fase 6 — Build de Producción

### 6.1 Íconos de la aplicación
- Crear `build/` en `src/electron-app/`
- Generar `icon.ico` (Windows, 256x256), `icon.icns` (macOS), `icon.png` (Linux, 512x512)
- Usar placeholder de color sólido indigo con siglas "GVP" en blanco centradas

### 6.2 electron-builder.yml para producción
- Verificar `dist/` y `dist-electron/` en `files`
- Agregar `extraResources` para incluir backend empaquetado:
  ```yaml
  extraResources:
    - from: backend-publish
      to: backend
  ```
- Configurar NSIS: `oneClick: false`, `allowToChangeInstallationDirectory: true`, `createDesktopShortcut: true`

### 6.3 Script `scripts/build.bat`
```bat
@echo off
title Sistema GVP - Build Produccion
cd /d "%~dp0.."

echo [1/3] Publicando backend .NET (win-x64 standalone)...
dotnet publish src\SistemaGVP.API\SistemaGVP.API.csproj -c Release --self-contained -r win-x64 -o src\electron-app\backend-publish
if %errorlevel% neq 0 (echo ERROR: Fallo la publicacion del backend. & pause & exit /b 1)
echo OK.

echo [2/3] Compilando frontend Electron + empaquetando instalador...
cd src\electron-app
call npm run electron:build
if %errorlevel% neq 0 (echo ERROR: Fallo el build de Electron. & cd ..\.. & pause & exit /b 1)
cd ..\..

echo [3/3] Build completado.
echo Instalador: src\electron-app\release\Sistema GVP Setup *.exe
pause
```

### 6.4 Actualizar `scripts/start.bat`
- Modo 3 nuevo: "Produccion local" → ejecuta `src\electron-app\backend-publish\SistemaGVP.API.exe` + `src\electron-app\release\win-unpacked\Sistema GVP.exe`

### 6.5 Ajustar `backend.ts` para rutas de producción
- En prod, `backendPath` debe ser `join(process.resourcesPath, 'backend')` (electron-builder copia `backend-publish/` a `resources/backend/`)
- Verificar que el path existe antes de spawn

---

## Fase 7 — Sistema de Licencias

### 7.1 Script `scripts/generate-keys.js`
- Genera par RSA 2048-bit: `private.pem` (NO commitear) y `public.pem`
- Guarda en `scripts/`

### 7.2 Completar `license.ts`
- Reemplazar `PUBLIC_KEY` placeholder por contenido real de `public.pem`
- La validación de firma ya está implementada, solo cambiar la clave

### 7.3 Script `scripts/generate-license.js`
```
Uso: node generate-license.js --company "Ferretería X" --machineId "ABC123" [--expiresAt "2027-06-20"]
```
- Carga `private.pem`
- Construye JSON: `{ machineId, companyName, issuedAt, expiresAt?, signature }`
- Firma con SHA256 + RSA + private key
- Serializa a base64
- Guarda como `license-{company}.txt` listo para entregar al cliente

### 7.4 Pantalla de activación de licencia
- Nueva ruta `/activate` (sin ProtectedRoute, accesible sin login)
- `App.tsx`: al iniciar, `getLicenseStatus()`. Si `valid: false` y `trial: false` (expirado), redirigir a `/activate`
- Durante trial, mostrar banner en header con días restantes
- Pantalla `/activate`: campo textarea para pegar clave base64, botón "Activar", resultado success/error

### 7.5 `.gitignore` para claves
```
scripts/private.pem
scripts/*.lic
```

---

## Fase 8 — Pulido y Deuda Técnica

### 8.1 Paginación en todas las pantallas
- Usar `DataTable` (1.8) con props de paginación en: SalesHistoryScreen, InventoryScreen, AuditLogScreen, BackupScreen
- CategoriesScreen y SuppliersScreen pueden no necesitar paginación inmediata (menos de 100 items típicamente)

### 8.2 Filtros funcionales
- `SalesHistoryScreen`: filtros por fecha (desde/hasta), método de pago (select), estado (select). Botón Filter abre panel de filtros.
- `AuditLogScreen`: filtros por entidad (select), acción (select), usuario, fecha
- `InventoryScreen`: filtros por tipo (select), producto (SearchInput), fecha

### 8.3 Restauración de backups
- `BackupScreen`: conectar botón Restaurar → `ConfirmDialog` con warning ("Esto sobreescribirá la base de datos actual") → `backupService.restore(fileName)` → toast
- Conectar botón Verificar → `backupService.getInfo(fileName)` → modal con info (fecha, tamaño, SHA256, creado por)

### 8.4 `.gitignore` completo
Agregar:
```
# Frontend dependencies
src/electron-app/node_modules/

# Build output
src/electron-app/dist/
src/electron-app/dist-electron/
src/electron-app/release/
src/electron-app/backend-publish/

# License keys
scripts/private.pem
scripts/*.lic

# Runtime artifacts
api.err
api.log
```

### 8.5 Actualizar `README.md`
- Título: "Sistema GVP POS — Electron + React + .NET 8"
- Sección inicio rápido: modo dev (API + Vite) y modo producción (build.bat)
- Estructura de carpetas actualizada (sin WPF, con API y electron-app)
- Credenciales default: mantener las existentes

### 8.6 Toast en todas las operaciones
Revisar cada pantalla y asegurar que create, update, delete, restore, backup, void, hold, resume, activate license, change password muestren toast success/error.

### 8.7 `SalesScreen`: opción de quedarse después de cobrar
- Actualmente `handleCheckout` navega a `/sales-history`. Agregar opción: si el cajero está en modo "venta continua", limpiar carrito y mantener foco en búsqueda sin navegar.

---

## Orden de Ejecución

```
Fase 1 ──► Fase 2 ──► Fase 3 ──► Fase 5 ──► Fase 6
  │         │            │            │
  │         │            └──► Fase 4 ─┘
  │         │
  │         └──► Fase 7 (independiente después de 2.9)
  │
  └──► Fase 8 (depende de todas)
```

**Dependencias:**
- Fase 1 es prerequisito de todo (componentes base, DataTable, useApi)
- Fase 2 depende de Fase 1 (modales + DataTable + FormField)
- Fase 3 depende de Fase 1 y 2 (sales usa modales, toasts, useApi)
- Fase 4 puede ejecutarse tras Fase 1 (chartCard + recharts son independientes, necesita useApi)
- Fase 5 depende de Fase 3 (ticket usa datos de venta y carrito con unidades)
- Fase 6 depende de Fase 3 y 5 (build listo cuando ventas + impresión funcionan)
- Fase 7 es independiente después de Fase 2.9 (ChangePasswordScreen comparte patrón con Fase 2)
- Fase 8 es final

---

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| `@tanstack/react-table` puede tener overhead de aprendizaje | Usar solo sorting y paginación básica, no features avanzados |
| La impresión por navegador puede abrir diálogo en algunas impresoras | Usar `silent: true` en `webContents.print()`, documentar que requiere impresora predeterminada configurada |
| El backend standalone (.exe) puede ser grande (~70MB con runtime) | Aceptable para distribución local, comprimir con NSIS |
| `electron-builder` sin GitHub releases no puede usar auto-update | Dejar `electron-updater` con `autoDownload: false` |
| Tasa de IVA puede desincronizarse si Settings cambia con el carrito abierto | El carrito usa la tasa al momento de calcular `total()`. Si cambia mid-venta, se actualiza al recargar. Aceptable. |
| `recharts` AreaChart de ventas diarias requiere datos que el endpoint `/api/dashboard/summary` podría no devolver | Si el endpoint no incluye `dailySales`, usar `GET /api/reports/sales` con rango de 7 días como fallback |

---

## Validación

- Crear un producto con unidad "kg", editarlo, eliminarlo → modales + toasts + DataTable con sorting
- Hacer una venta con 0.5 kg de tornillos + 2 m de cable → carrito muestra unidades → cobrar → ticket impreso con "0.5 kg" y "2 m"
- Pausar una venta, reanudarla, completarla
- Seleccionar cliente con saldo cerca del límite → confirmDialog de advertencia
- Cambiar IVA en Settings → abrir nueva venta → verificar que calcula con nueva tasa
- Anular una venta desde el historial (admin)
- Reimprimir ticket desde el historial
- Login con `mustChangePassword: true` → redirige a `/change-password` → cambiar → login normal
- Ejecutar `scripts\build.bat` → `SistemaGVP-Setup-x64.exe` funcional
- `node scripts/generate-keys.js` → `node scripts/generate-license.js --company "Test" --machineId "ABC"` → pegar clave en pantalla `/activate` → licencia aceptada
