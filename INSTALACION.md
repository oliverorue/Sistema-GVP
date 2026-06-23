# Instalación — Sistema GVP POS

Guía paso a paso para instalar y configurar el sistema.

---

## Requisitos del Sistema

| Componente | Requisito |
|------------|-----------|
| **Sistema Operativo** | Windows 10/11 (64-bit) o Linux |
| **Procesador** | 1.5 GHz o superior |
| **Memoria RAM** | 4 GB mínimo, 8 GB recomendado |
| **Disco** | 500 MB libres |
| **.NET Runtime** | Incluido en el instalador (no requiere instalación aparte) |
| **Impresora** | Cualquier impresora térmica o matricial configurada como predeterminada |

---

## Instalación — Windows

1. **Ejecutar el instalador**: haz doble clic en `Sistema-GVP-2.0.0-Setup.exe`
2. Aparece el asistente de instalación. Haz clic en **Siguiente**.
3. Elige la carpeta de instalación (deja la que sugiere el instalador).
4. Marca **"Crear acceso directo en el escritorio"**.
5. Haz clic en **Instalar** y espera ~30 segundos.
6. Haz clic en **Finalizar**.
7. **Iniciar la app**: haz doble clic en el ícono del escritorio.
8. El sistema arranca el servidor interno (tarda ~10 segundos) y muestra la pantalla de inicio de sesión.

---

## Instalación — Linux

1. Copia `Sistema-GVP-2.0.0.AppImage` a la computadora (p.ej. el escritorio).
2. Dale permiso de ejecución:
   - Clic derecho → Propiedades → Permisos → "Permitir ejecutar como programa"
   - O en terminal: `chmod +x Sistema-GVP-2.0.0.AppImage`
3. Haz doble clic en el archivo. El sistema se inicia automáticamente.
4. La base de datos se crea en la primera ejecución.

---

## Primer Inicio de Sesión

| Campo | Valor |
|-------|-------|
| **Usuario** | `admin` |
| **Contraseña** | `admin123` |

El sistema solicitará cambiar la contraseña en el primer inicio de sesión.

Usuario cajero de prueba:

| Campo | Valor |
|-------|-------|
| **Usuario** | `cajero1` |
| **Contraseña** | `cajero123` |

El sistema incluye datos de demostración (categorías, productos, clientes) que pueden eliminarse desde el panel de administración.

---

## Configuración Inicial

1. **Datos de la empresa**: Menú Configuración → completar nombre, RUC, dirección, teléfono
2. **IVA**: Configurar si los precios incluyen IVA o no
3. **Categorías**: Crear las categorías de productos
4. **Productos**: Cargar el inventario inicial (costo y precio de venta)
5. **Clientes**: Si manejan crédito, cargar los clientes con límite

---

## Respaldo (Backup)

- El sistema realiza backup automático cada 4 horas
- Backup manual disponible desde el menú **Backup**
- Los backups se almacenan en la carpeta `Backups/` junto al programa
- Se recomienda copiar los backups a un pendrive o ubicación externa periódicamente

---

## Solución de Problemas

| Problema | Solución |
|----------|----------|
| La app no abre | Cerrar y volver a abrir. Si persiste, reinstalar. |
| Pantalla en blanco o error de conexión | El servidor interno no arrancó. Cerrar y reabrir la app. |
| El puerto 5000 está ocupado | Cerrar cualquier programa que use el puerto 5000. Si el problema persiste, reiniciar la PC. |
| El backend no arranca | Verificar que no haya otra instancia ejecutándose. Revisar el firewall. |
| Error de permisos en Program Files | Ejecutar la app como administrador una vez (clic derecho → "Ejecutar como administrador"). |
| No imprime | Verificar que la impresora esté encendida, conectada y configurada como predeterminada. |
| Olvidé la contraseña | Contactar al soporte técnico para restablecer el usuario administrador. |
| Se cortó la luz | Al reabrir la app los datos no se pierden. El sistema usa SQLite con escritura atómica. |

---

## Desinstalación

### Windows
1. Abrir **Configuración** → **Aplicaciones** → **Aplicaciones instaladas**
2. Buscar "Sistema GVP" y hacer clic en **Desinstalar**
3. Confirmar la desinstalación
4. (Opcional) Eliminar la carpeta `%APPDATA%\sistema-gvp` para borrar la base de datos y configuración

### Linux
Eliminar el archivo AppImage y la carpeta de configuración en `~/.config/sistema-gvp`.

---

## Actualizaciones

1. Descargar la nueva versión del instalador
2. Ejecutar el instalador sobre la instalación existente
3. Los datos se conservan (la base de datos está en `%APPDATA%`, no en la carpeta del programa)

---

*Sistema GVP — Punto de Venta*
