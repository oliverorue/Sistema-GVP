# Plan: Ferretería Paraguaya + IVA Incluido + Dashboard Completo

## Objetivo

Transformar el Sistema GVP de demo genérica (Coca-Cola, arroz) a un sistema listo para ferretería paraguaya con IVA 10% incluido, productos reales, escáner de código de barras funcional, y dashboard completo navegable.

---

## Fase 1 — Backend: Semilla Ferretería + IVA Incluido

### 1.1 Agregar `IvaIncluido` a Company

**Archivo:** `src/SistemaGVP.Domain/Entities/Company.cs`
- Agregar propiedad: `public bool IvaIncluido { get; set; } = true;`

**Archivo:** `src/SistemaGVP.Infrastructure/Data/Configurations/CompanyConfiguration.cs` (o crear si no existe)
- Configurar EF mapping para el nuevo campo

**Crear migración:**
```bash
dotnet ef migrations add AddIvaIncluidoToCompany \
  --project src/SistemaGVP.Infrastructure \
  --startup-project src/SistemaGVP.API
```

### 1.2 Reemplazar semilla por productos de ferretería

**Archivo:** `src/SistemaGVP.Infrastructure/Data/Seed/DatabaseSeeder.cs`

**Categorías (reemplazar Bebidas/Alimentos/Lácteos/Limpieza/Electrónicos):**
```csharp
"Fijaciones", "Materiales", "Electricidad", "Pinturas", "Caños y Tubos", "Ferretería General"
```

**Proveedores (reemplazar Distribuidora XYZ/Mayorista ABC):**
```csharp
"Ferremix S.A.", "Distribuidora Paraguay S.R.L."
```

**Clientes (mantener + agregar):**
```csharp
"Consumidor Final", "Constructor Obras S.A.", "Arq. Martínez"
```

**Productos ferretería demo (18 productos):**

| Categoría | Nombre | Código | Precio | Costo | Stock | Unidad |
|-----------|--------|--------|--------|-------|-------|--------|
| Fijaciones | Clavo 2 pulgadas | FIJ-001 | 12.000 | 8.500 | 25 | kg |
| Fijaciones | Tornillo 8mm x 50mm | FIJ-002 | 150 | 90 | 500 | pz |
| Fijaciones | Tarugo plástico 6mm | FIJ-003 | 100 | 50 | 1000 | pz |
| Materiales | Cemento CPC-40 | MAT-001 | 55.000 | 42.000 | 100 | bolsa |
| Materiales | Arena lavada | MAT-002 | 85.000 | 55.000 | 50 | m³ |
| Materiales | Cal hidratada 20kg | MAT-003 | 25.000 | 18.000 | 60 | bolsa |
| Electricidad | Cable 2.5mm | ELE-001 | 3.500 | 2.200 | 200 | m |
| Electricidad | Enchufe bipolar 10A | ELE-002 | 8.500 | 5.500 | 150 | pz |
| Electricidad | Cinta aisladora | ELE-003 | 5.000 | 2.800 | 80 | pz |
| Pinturas | Látex interior blanco 20L | PIN-001 | 180.000 | 130.000 | 40 | pza |
| Pinturas | Esmalte sintético 1GL | PIN-002 | 95.000 | 68.000 | 30 | pza |
| Pinturas | Pincel 2 pulgadas | PIN-003 | 15.000 | 8.500 | 60 | pz |
| Caños y Tubos | Caño PVC 1/2" | CAÑ-001 | 6.500 | 4.200 | 100 | m |
| Caños y Tubos | Codo PVC 90° 1/2" | CAÑ-002 | 2.500 | 1.300 | 200 | pz |
| Caños y Tubos | Tee PVC 1/2" | CAÑ-003 | 3.500 | 1.800 | 150 | pz |
| Ferretería Gral | Candado 30mm | FER-001 | 22.000 | 14.000 | 45 | pz |
| Ferretería Gral | Bisagra 3" | FER-002 | 4.500 | 2.500 | 80 | par |
| Ferretería Gral | Lija N°100 | FER-003 | 3.500 | 1.800 | 120 | pz |

Precios **con IVA 10% incluido**. El costo es sin IVA (para calcular margen).

### 1.3 Modificar cálculo de IVA en SaleService

**Archivo:** `src/SistemaGVP.Application/Services/SaleService.cs`

Lógica actual (líneas 449-452): siempre calcula IVA sobre subtotal
```csharp
var tax = (subtotal - totalDiscount) * taxRate;
var total = subtotal - totalDiscount + tax;
```

**Nueva lógica:**
```csharp
if (company?.IvaIncluido == true)
{
    // IVA incluido: los precios ya contienen IVA
    // IVA = total_con_iva / 1.10 * 0.10 = total_con_iva / 11
    var baseAmount = (subtotal - totalDiscount) / (1 + taxRate);
    tax = (subtotal - totalDiscount) - baseAmount;
    total = subtotal - totalDiscount;
}
else
{
    // IVA discriminado: se suma al subtotal
    tax = (subtotal - totalDiscount) * taxRate;
    total = subtotal - totalDiscount + tax;
}
```

### 1.4 Actualizar SettingsEndpoints para IvaIncluido

**Archivo:** `src/SistemaGVP.API/Endpoints/SettingsEndpoints.cs`

Asegurar que el GET devuelve `IvaIncluido` y el PUT lo persiste. Verificar que el `CompanyDto` incluya el campo.

### 1.5 DashboardEndpoints más completo

**Archivo:** `src/SistemaGVP.API/Endpoints/DashboardEndpoints.cs`

Agregar a la respuesta del `GET /api/dashboard/summary`:
- `heldSalesCount` — count de ventas con status HeldSale
- `customerCount` — count de clientes activos
- `productCount` — count de productos activos
- `lowStockProducts` — lista de productos bajo stock mínimo (no solo el número)

Inyectar `ISaleRepository` y `IRepository<Customer>` en el endpoint.

---

## Fase 2 — Frontend: IVA, Configuración, Ticket, Formato

### 2.1 Agregar `ivaIncluido` al tipo Company

**Archivo:** `src/electron-app/src/types/entities.ts`
```typescript
export interface Company {
  // ... existing fields
  ivaIncluido: boolean
}
```

### 2.2 Toggle "IVA Incluido" en SettingsScreen

**Archivo:** `src/electron-app/src/screens/SettingsScreen.tsx`

Agregar debajo de "Impuesto (%)":
```tsx
<div>
  <label className="block text-sm font-medium text-slate-700 mb-2">IVA en precios</label>
  <label className="relative inline-flex items-center cursor-pointer">
    <input type="checkbox" checked={company?.ivaIncluido ?? true} 
      onChange={(e) => setCompany(prev => prev ? {...prev, ivaIncluido: e.target.checked} : null)} 
      className="sr-only peer" />
    <div className="w-11 h-6 bg-slate-200 peer-focus:ring-2 peer-focus:ring-indigo-300 rounded-full peer peer-checked:bg-indigo-600 peer-checked:after:translate-x-full after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all"></div>
    <span className="ml-3 text-sm text-slate-700">IVA incluido en precios</span>
  </label>
  <p className="text-xs text-slate-400 mt-1">Si está activo, los precios en góndola ya incluyen el IVA (10%)</p>
</div>
```

### 2.3 Ajustar cartStore según IvaIncluido

**Archivo:** `src/electron-app/src/stores/cartStore.ts`

Agregar campo `ivaIncluido: true` al estado. Actualizar cálculos:
```typescript
tax: () => {
  const s = get()
  if (s.ivaIncluido) {
    const base = (s.subtotal() - s.discount) / (1 + s.taxRate)
    return (s.subtotal() - s.discount) - base
  }
  return s.subtotal() * s.taxRate
},
total: () => {
  const s = get()
  if (s.ivaIncluido) return s.subtotal() - s.discount
  return s.subtotal() + s.tax() - s.discount
}
```

Cargar `ivaIncluido` desde `settingsService.getCompany()` en `AppLayout.tsx` (donde ya se carga `taxRate`).

### 2.4 Ticket con IVA claro

**Archivo:** `src/electron-app/src/components/shared/TicketTemplate.tsx`

Agregar en la sección de totales:
```html
<tr><td class="label">Base imponible</td><td class="value">${formatCurrency(baseAmount)}</td></tr>
<tr><td class="label">IVA 10% ${company.ivaIncluido ? '(incluido)' : ''}</td><td class="value">${formatCurrency(sale.tax)}</td></tr>
```

Y en la info de items, mostrar la unidad:
```html
<td class="qty">${item.quantity} ${item.unit || ''} x ${item.productName}</td>
```

### 2.5 formatCurrency con soporte decimal

**Archivo:** `src/electron-app/src/utils/format.ts`

```typescript
export function formatCurrency(amount: number, currency = 'Gs.'): string {
  const decimals = amount % 1 === 0 ? 0 : 0
  return `${currency} ${amount.toLocaleString('es-PY', { 
    minimumFractionDigits: 0, 
    maximumFractionDigits: 3 
  })}`
}
```

Mantener `maximumFractionDigits: 3` para que cantidades como `Gs. 12.500` se muestren correctamente, pero valores enteros como `Gs. 55.000` se muestren sin decimales.

---

## Fase 3 — Frontend: Bugs y Mejoras Ferretería

### 3.1 Agregar `Unit` a SaleDetail (entidad + DTO + frontend)

**Problema:** SaleDetail no guarda la unidad (kg/m/pz), entonces el ticket no puede mostrar "0.5 kg x Clavos".

**Cambios:**
1. `SaleDetail.cs` (Domain) — agregar `public string Unit { get; set; } = "pz";`
2. `SaleDetailDto.cs` (Application) — agregar `public string Unit { get; set; } = "pz";`
3. `SaleService.cs` — en `CreateSaleAsync`, copiar `Unit = product.Unit`
4. `entities.ts` (frontend) — agregar `unit: string` a `SaleDetail`
5. `.kilo/migration` — crear migración `AddUnitToSaleDetail`

### 3.2 29 catch {} vacíos → Logger.error()

**Archivos afectados (14 archivos):**
SalesScreen, SalesHistoryScreen, ProductsScreen, CategoriesScreen, CustomersScreen, InventoryScreen, ReportsScreen, BackupScreen, AuditLogScreen, ActivateScreen, ChangePasswordScreen, StatusBar, usePrintTicket

**Cambio:** Reemplazar todo `catch { }` por:
```typescript
catch (err) {
  Logger.error('ModuleName', 'Descripción del error', err)
}
```

### 3.3 Integrar scanner.ts en main.ts + SalesScreen

**Archivo:** `src/electron-app/electron/main.ts`
- Importar y llamar `setupScanner()` (igual que se hizo con `setupPrinter()`)

**Archivo:** `src/electron-app/src/screens/SalesScreen.tsx`
- Escuchar evento `scanner:barcode` del preload/IPC
- Al recibir un barcode, hacer `productService.search(barcode)` y si es match exacto, agregar al carrito automáticamente
- Agregar al `electronAPI` en `preload.ts` y `electron.d.ts`

### 3.4 Validación de crédito en backend

**Archivo:** `src/SistemaGVP.Application/Services/SaleService.cs`

En `CreateSaleAsync`, después de validar stock, agregar:
```csharp
if (dto.CustomerId.HasValue)
{
    var customer = await _customerRepository.GetByIdAsync(dto.CustomerId.Value);
    if (customer != null && customer.CreditLimit > 0 
        && (total + customer.Balance > customer.CreditLimit))
    {
        return ServiceResult<SaleDto>.Failure(
            $"El cliente '{customer.Name}' supera su límite de crédito. Saldo: {customer.Balance:N0}, Límite: {customer.CreditLimit:N0}");
    }
}
```

### 3.5 Validación de stock en frontend antes del checkout

**Archivo:** `src/electron-app/src/screens/SalesScreen.tsx` — `handleCheckout`

Antes de llamar a `saleService.create()`, verificar que cada item del carrito tenga suficiente stock consultando `productService.search()` o el resultado más reciente. Mostrar alerta si algún producto no tiene stock.

---

## Fase 4 — Dashboard Navegable

### 4.1 Stats cards clickeables

Envolver cada card con `useNavigate()`:

```tsx
import { useNavigate } from 'react-router-dom'
const navigate = useNavigate()

// Cada card:
<div onClick={() => navigate('/sales')} className="... cursor-pointer hover:scale-[1.02] transition-transform">
```

| Card | Navega a |
|------|----------|
| Ventas Hoy | `/sales` |
| Ingresos Hoy | `/reports` |
| Stock Bajo | `/inventory` |
| Ticket Promedio | `/reports` |
| Ventas en Espera (nueva) | `/sales` |
| Clientes (nueva) | `/customers` |
| Productos (nueva) | `/products` |

### 4.2 Nuevas cards

Agregar 3 cards adicionales: `heldSalesCount`, `customerCount`, `productCount`.

### 4.3 Últimos Movimientos (sección nueva)

Debajo de los gráficos, agregar una tabla con los últimos 5 `recentMovements` (ya vienen del backend):
- Columna: Fecha, Producto, Tipo (badge), Cantidad
- Link "Ver todos →" que navega a `/inventory`

### 4.4 Stock Bajo Detalle

Si `lowStockCount > 0`, mostrar lista de productos con stock bajo (nombre, stock actual, stock mínimo) debajo del card. Link "Ir a inventario →".

### 4.5 Gráficos con links

- "Productos más vendidos" → link "Ver productos →" a `/products`
- "Ventas Diarias" → link "Ver historial →" a `/sales-history`

---

## Orden de Ejecución

| Paso | Fase | Archivos | Backend/Frontend |
|------|------|----------|-----------------|
| 1 | 1.1 | `Company.cs` + migración | Backend |
| 2 | 1.2 | `DatabaseSeeder.cs` | Backend |
| 3 | 1.3 | `SaleService.cs` | Backend |
| 4 | 1.4 | `SettingsEndpoints.cs` | Backend |
| 5 | 1.5 | `DashboardEndpoints.cs` | Backend |
| 6 | 3.4 | `SaleService.cs` (crédito) | Backend |
| 7 | — | `dotnet build` | Backend |
| 8 | 2.1 | `entities.ts` | Frontend |
| 9 | 2.2 | `SettingsScreen.tsx` + `settingsService.ts` | Frontend |
| 10 | 2.3 | `cartStore.ts` + `AppLayout.tsx` | Frontend |
| 11 | 2.4 | `TicketTemplate.tsx` | Frontend |
| 12 | 2.5 | `format.ts` | Frontend |
| 13 | 3.1 | `SaleDetail.cs` + migración + DTO + entities.ts | Backend + Frontend |
| 14 | 3.2 | 14 archivos `catch {}` | Frontend |
| 15 | 3.3 | `main.ts` + `SalesScreen.tsx` + `preload.ts` + `electron.d.ts` | Frontend |
| 16 | 3.5 | `SalesScreen.tsx` (stock check) | Frontend |
| 17 | 4.1-4.5 | `DashboardScreen.tsx` | Frontend |
| 18 | — | `npx tsc` + `npm run lint` + `npm run build` | Frontend |
| 19 | — | Eliminar DB vieja + reiniciar API | Ambos |

---

## Validación

- [ ] `dotnet build` sin errores
- [ ] `npx tsc --noEmit` sin errores
- [ ] `npm run lint` 0 errores, 0 warnings
- [ ] `npm run build` sin errores
- [ ] Login con `admin/admin123` muestra productos de ferretería
- [ ] Pantalla Configuración muestra toggle IVA incluido
- [ ] Crear venta: ticket muestra unidad (kg/m/pz) y base imponible + IVA
- [ ] Dashboard: cards clickeables navegan a secciones correctas
- [ ] Dashboard: muestra ventas en espera, clientes, productos, últimos movimientos
- [ ] Dashboard: stock bajo muestra lista de productos
- [ ] Escáner de código de barras funciona en Electron
- [ ] Crédito de cliente se valida en backend
- [ ] `catch {}` vacíos muestran errores en Logger
