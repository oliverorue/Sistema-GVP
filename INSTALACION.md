# INSTALACIÓN Y VENTA — Sistema GVP POS

**Guía para el vendedor/revendedor**

---

## 📦 Lo que obtenés al buildear

| Archivo | Sistema | Peso aprox. |
|---------|---------|------------|
| `Sistema-GVP-2.0.0.AppImage` | Linux | ~120 MB |
| `Sistema-GVP-2.0.0-Setup.exe` | Windows | ~130 MB |

Ambos son **autónomos** — el cliente no necesita instalar .NET, Node.js ni nada.

---

## 🏗️ Cómo buildear (vos, el vendedor)

### Requisitos en tu PC

| Herramienta | Para qué |
|------------|----------|
| **.NET SDK 8.0** | Compilar el backend |
| **Node.js 20+** | Compilar el frontend y Electron |
| **npm** | Viene con Node.js |

### Paso 1 — Poner el logo

Reemplazá el archivo:
```
src/electron-app/build/icon.png
```
Con tu logo de **256×256 píxeles PNG**. Si no tenés, dejá el placeholder (sale un ícono azul genérico).

### Paso 2 — Buildear

```bash
# Linux
scripts/build.sh linux

# Windows
scripts/build.sh win

# Ambos a la vez
scripts/build.sh all
```

Tarda ~3-5 minutos. El resultado queda en:
```
src/electron-app/release/
```

---

## 💰 Cómo venderlo

### Modelo sugerido

| Concepto | Precio sugerido |
|----------|----------------|
| Licencia única (una PC) | Gs. 500.000 — 1.000.000 |
| Licencia 2 PCs | Gs. 800.000 — 1.500.000 |
| Soporte mensual | Gs. 50.000 — 100.000 |
| Capacitación (1-2 hs) | Gs. 150.000 — 300.000 |

### Qué entregás al cliente

1. **El instalador** (AppImage o .exe)
2. **Usuario y contraseña inicial** (admin / la que configures)
3. **Capacitación** (opcional, muy recomendada)
4. **MANUAL.md** impreso o en PDF

### Proceso de venta típico

1. **Demostración**: mostrale la app funcionando en tu laptop
2. **Cierre**: acordá el precio y forma de pago
3. **Instalación**: copiás el instalador a su PC (pendrive o descarga)
4. **Capacitación**: 1-2 horas enseñándole lo básico (venta, productos, backup)
5. **Soporte**: quedás disponible por teléfono/WhatsApp para dudas

---

## 💻 Instalación — Paso a paso para el cliente

### Opción A — Linux

1. **Copiar el archivo** `Sistema-GVP-2.0.0.AppImage` a la computadora (escritorio)
2. **Dar permiso de ejecución** (solo la primera vez):
   - Clic derecho → Propiedades → Permisos → "Permitir ejecutar como programa"
   - O en terminal: `chmod +x Sistema-GVP-2.0.0.AppImage`
3. **Doble clic** en el archivo
4. **Listo** — el sistema arranca solo. La base de datos se crea automáticamente.

### Opción B — Windows

1. **Doble clic** en `Sistema-GVP-2.0.0-Setup.exe`
2. Aparece el instalador. Clic en **Siguiente**.
3. Elegí la carpeta donde instalar (dejá la que sugiere).
4. Marcá **"Crear acceso directo en el escritorio"**.
5. Clic en **Instalar** → esperá ~30 segundos.
6. Clic en **Finalizar**.
7. **Doble clic** en el ícono del escritorio.
8. **Listo** — el sistema arranca solo.

### Qué pasa al abrir por primera vez

1. El sistema inicia el servidor interno (tarda ~10 segundos).
2. La base de datos se crea automáticamente.
3. Aparece la pantalla de **Iniciar Sesión**.

---

## 🔑 Primer inicio de sesión

| Campo | Valor |
|-------|-------|
| **Usuario** | `admin` |
| **Contraseña** | `admin123` |

**⚠️ El sistema le va a pedir que cambie la contraseña la primera vez que inicia sesión.**

También hay un usuario cajero de prueba:

| Campo | Valor |
|-------|-------|
| **Usuario** | `cajero1` |
| **Contraseña** | `cajero123` |

### Datos de demostración incluidos

El sistema ya viene con datos de ejemplo para que el cliente vea cómo funciona:

| Dato | Cantidad |
|------|---------|
| Categorías | 6 (Fijaciones, Materiales, Electricidad, Pinturas, Caños, Ferretería) |
| Proveedores | 2 |
| Clientes | 3 (incluye uno con crédito) |
| Productos | 18 con stock y precios |
| Empresa demo | "Ferretería Paraguaya S.A." |

El cliente puede **borrar todo esto** y cargar sus propios datos, o usarlo como base.

---

## ⚙️ Configuración inicial para el cliente

Apenas instalado, configurá esto con el cliente:

1. **Datos de la empresa**: Configuración → llenar nombre, RUC, dirección, teléfono (aparece en tickets)
2. **IVA**: Configurar si los precios incluyen IVA o no
3. **Categorías**: Crear las categorías de productos (Caños, Ferretería, Pinturas...)
4. **Productos**: Cargar el inventario inicial (con costo y precio de venta)
5. **Clientes**: Si manejan crédito, cargar los clientes con límite

---

## 🛡️ Respaldo (backup)

**Enseñale esto al cliente el primer día:**

- El sistema hace backup **automático cada 4 horas**
- También puede hacer backup manual desde el menú **Backup**
- **Copiar los backups a un pendrive** una vez por semana
- Los backups están en la carpeta `Backups/` junto al programa

---

## 🆘 Soporte post-venta

### Problemas comunes y solución

| Problema | Solución |
|----------|---------|
| "No abre" | Cerrar y volver a abrir. Si persiste, reinstalar. |
| "Error de conexión" | El servidor interno falló. Cerrar y reabrir. |
| "No imprime" | Verificar que la impresora esté conectada y tenga papel. |
| "Olvidé la contraseña" | Hay que editar la BD o recrear el usuario. (Soporte técnico) |
| "Se cortó la luz" | Al volver, abrir normalmente. Los datos no se pierden. |

---

## 📈 Estrategia de venta

### A quién venderle

- Ferreterías de barrio (1-3 empleados)
- Minimercados y almacenes
- Librerías
- Cualquier comercio que venda productos físicos

### Cómo hacer la demo

1. **Creá un producto** (ej: "Lija N°100")
2. **Vendelo** (buscá el producto, agregá al carrito, cobrá)
3. **Mostrá el ticket**
4. **Mostrá el dashboard** con la venta del día
5. **Creá un cliente con crédito** y mostrá cómo se vende fiado
6. **Mostrá el backup** — "nunca pierde los datos"

### Diferenciadores (qué decir)

- ✅ **No necesita internet** — funciona hasta en el campo
- ✅ **No necesita servidor** — todo en una PC
- ✅ **Backup automático** — nunca pierde datos
- ✅ **Crédito y cobranza** — ideal para clientes que compran fiado
- ✅ **Factura electrónica** (si se configura)
- ✅ **Sin costo mensual** — se paga una sola vez

---

## 🔄 Actualizaciones

Cuando saques una nueva versión:

1. Buildear con el nuevo número de versión (cambiar en `package.json`)
2. Entregar el nuevo instalador al cliente
3. El cliente instala encima (los datos se mantienen porque la BD está en otra carpeta)

---

*Sistema GVP — Punto de Venta*
