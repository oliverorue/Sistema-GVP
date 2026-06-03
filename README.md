# 🏪 Sistema GVP POS — WPF (.NET 8)

Sistema de Punto de Venta (POS) moderno desarrollado en C# con **WPF .NET 8** y **SQLite** para **Windows**.

---

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10/11

### Primera configuración (solo una vez)

Ejecutá desde la raíz del proyecto:

`cmd
scripts\setup.bat
`

Esto restaura paquetes, compila la solución y crea la base de datos SQLite.

### Inicio diario

`cmd
scripts\start.bat
`

O manualmente:

`cmd
dotnet run --project src\SistemaGVP.WPF\SistemaGVP.WPF.csproj
`

---

## ✨ Características

### Gestión
- Productos, Categorías, Clientes y Proveedores
- Control de stock con alertas de stock bajo
- Código de barras y precios de venta/compra

### Ventas
- Ventas con múltiples productos y métodos de pago (Efectivo/Tarjeta/Transferencia)
- **Ventas en Espera**: guardar y reanudar ventas pendientes
- **Historial**: buscar ventas por fecha, producto, método de pago
- **Anulación**: cancelar ventas con reversión automática de stock

### Reportes
- Stock bajo, ventas por período, margen de ganancia, inventario valorizado
- Exportación a Excel (CSV) y PDF (HTML imprimible)

### Administración
- **Usuarios** con roles (Admin/Cajero)
- **Configuración**: impuestos, moneda, umbral de stock
- **Backup y Restauración** con verificación SHA256
- **Auditoría**: bitácora completa de acciones del sistema

---

## 🔑 Credenciales por Defecto

| Usuario | Contraseña | Rol |
|---------|-----------|------|
| dmin | dmin123 | Administrador |
| cajero | cajero123 | Cajero |

> ⚠️ **Cambiá las contraseñas por defecto después del primer inicio.**

---

## 🗺️ Navegación

| Sección | Módulos | Quién puede verlo |
|---------|---------|-------------------|
| **VENTAS** | Nueva Venta, Historial | Todos |
| **GESTIÓN** | Productos, Categorías, Clientes | Todos |
| **INVENTARIO** | Movimientos, Alertas de Stock | Todos |
| **REPORTES** | Reportes y Exportación | Todos |
| **ADMINISTRACIÓN** | Proveedores | Todos |
| **ADMINISTRACIÓN** | Usuarios, Configuración, Backup, Auditoría | Solo Admin |

---

## 🛠️ Estructura del Proyecto

`
src/
├── SistemaGVP.Domain/          # Entidades y Enums
├── SistemaGVP.Application/     # DTOs, Servicios, Interfaces
├── SistemaGVP.Infrastructure/  # EF Core, Repositorios, SQLite
└── SistemaGVP.WPF/             # WPF (Vistas, ViewModels, Estilos)
`

## 📦 Tecnologías

| Tecnología | Propósito |
|-----------|-----------|
| .NET 8 + WPF | Framework de escritorio Windows |
| Entity Framework Core + SQLite | Base de datos local |
| CommunityToolkit.Mvvm | MVVM con Source Generators |
| AutoMapper | Mapeo DTO-Entidad |
| FluentValidation | Validación de datos |
| BCrypt | Hash de contraseñas |
| Serilog | Registro de eventos |
| LiveChartsCore | Gráficos interactivos |
| QRCoder | Generación de códigos QR |
| Microsoft.AspNetCore.App | Servidor HTTP embebido (Kestrel) para escáner móvil |

---

## 💡 Tips

- **Stock bajo**: Configurá el umbral en Configuración → LowStockThreshold
- **Backup**: Andá a Administración → Backup para crear copias de seguridad
- **Auditoría**: Todas las acciones quedan registradas automáticamente
- **Ventas en Espera**: Durante una venta, usá "Pausar" para guardarla y continuar después
- **Anular venta**: En Historial, seleccioná una venta y hacé clic en "Anular" (solo Admin)
- **Hot Reload**: Con Visual Studio 2022, ejecutá con F5 y editá cualquier .xaml en caliente

## 🗄️ Base de Datos

- SQLite local: sistemagvp.db (se crea automáticamente)
- Los backups se guardan en Backups/
- Datos de prueba incluidos (productos, cliente, usuario admin)

## 🧪 Solución de Problemas

**Error: "No se encuentra dotnet"**
→ Instalá .NET 8 SDK desde https://dotnet.microsoft.com/download/dotnet/8.0

**Error de base de datos**
→ Eliminá sistemagvp.db y ejecutá scripts\setup.bat de nuevo

**La app no se abre**
→ Asegurate de tener Windows 10/11 con .NET 8 SDK instalado
→ WPF no funciona en Linux/macOS

---

## 📄 Licencia

Uso interno - Todos los derechos reservados.
