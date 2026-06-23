# MANUAL DE USUARIO — Sistema GVP POS

**Versión 2.0**  
**Ferretería y Punto de Venta**

---

## 📖 Índice

1. [¿Qué es este sistema?](#1-qué-es-este-sistema)
2. [Cómo iniciar el sistema](#2-cómo-iniciar-el-sistema)
3. [Primeros pasos — Iniciar sesión](#3-primeros-pasos--iniciar-sesión)
4. [Pantalla principal (Dashboard)](#4-pantalla-principal-dashboard)
5. [Realizar una venta](#5-realizar-una-venta)
6. [Historial de ventas](#6-historial-de-ventas)
7. [Productos — Administrar inventario](#7-productos--administrar-inventario)
8. [Categorías](#8-categorías)
9. [Clientes — Manejo de crédito y cobranza](#9-clientes--manejo-de-crédito-y-cobranza)
10. [Proveedores](#10-proveedores)
11. [Inventario — Movimientos de stock](#11-inventario--movimientos-de-stock)
12. [Reportes](#12-reportes)
13. [Usuarios del sistema](#13-usuarios-del-sistema)
14. [Backups — Respaldo de datos](#14-backups--respaldo-de-datos)
15. [Auditoría — Quién hizo qué](#15-auditoría--quién-hizo-qué)
16. [Configuración](#16-configuración)
17. [Solución de problemas](#17-solución-de-problemas)
18. [Preguntas frecuentes](#18-preguntas-frecuentes)

---

## 1. ¿Qué es este sistema?

El **Sistema GVP** es un programa para manejar una ferretería o comercio. Sirve para:

- **Cobrar** a los clientes cuando compran algo
- **Controlar el stock** de los productos (cuántos hay, cuándo reponer)
- **Saber cuánto se vendió** en el día, la semana o el mes
- **Llevar cuenta** de clientes que compran fiado (crédito)
- **Generar reportes** para ver ganancias, productos más vendidos, etc.
- **Hacer respaldos** de toda la información por seguridad

**No necesita internet.** Todo funciona en su computadora.

---

## 2. Cómo iniciar el sistema

### Si ya está instalado

1. Busque el ícono **"Sistema GVP"** en el escritorio y haga doble clic
2. Espere unos segundos a que cargue
3. Listo — aparece la pantalla de inicio de sesión

### Si quiere abrir desde la terminal (técnico)

```bash
cd /ruta/al/sistema
scripts/start.sh
```

Esto abre el sistema en modo desarrollo. Para el usuario normal, basta con el ícono del escritorio.

---

## 3. Primeros pasos — Iniciar sesión

Cuando abre el sistema, ve una pantalla azul con dos campos:

| Campo | Qué escribir |
|-------|-------------|
| **Usuario** | Su nombre de usuario (ej: `admin`) |
| **Contraseña** | La contraseña que le dieron |

Haga clic en **Iniciar Sesión**.

### Si se equivoca

- Si pone mal el usuario o contraseña, aparece un mensaje en rojo: "Usuario o contraseña incorrectos"
- Vuelva a intentarlo

### Usuario administrador por defecto

| Campo | Valor |
|-------|-------|
| Usuario | `admin` |
| Contraseña | La que configuró al instalar |

---

## 4. Pantalla principal (Dashboard)

Después de iniciar sesión, ve la pantalla principal con:

| Sección | Qué significa |
|---------|--------------|
| **Ventas hoy** | Cuántas ventas se hicieron en el día y cuánta plata entró |
| **Stock bajo** | Productos que están por debajo del mínimo y necesitan reposición |
| **Productos top** | Los productos más vendidos |
| **Últimos movimientos** | Entradas y salidas de inventario recientes |
| **Productos activos** | Cuántos productos tiene en total |
| **Clientes** | Cantidad de clientes registrados |

A la izquierda está el **menú lateral** con todas las secciones. Haga clic en cualquier ícono para ir a esa sección.

---

## 5. Realizar una venta

Esta es la operación más importante. Siga estos pasos:

### 5.1 Buscar producto

1. Haga clic en **Ventas** (ícono de carrito) en el menú izquierdo
2. En la barra de búsqueda, escriba el nombre o código del producto
3. Aparece una lista — haga clic en el producto que quiere vender
4. El producto se agrega al carrito (lado derecho)

### 5.2 Cambiar cantidad

- Use los botones **+** y **-** para sumar o restar
- También puede **escribir directamente** la cantidad (ej: `2,5` para 2 unidades y media)

### 5.3 Agregar cliente (opcional)

- Arriba a la derecha hay un botón para buscar cliente
- Si el cliente compra fiado (crédito), **asígnelo aquí**
- El sistema verifica que no supere su límite de crédito

### 5.4 Cobrar

1. Haga clic en **Cobrar** (botón verde grande)
2. Se abre una ventana con:

| Campo | Qué hacer |
|-------|----------|
| **Método de pago** | Elija: Efectivo, Tarjeta, Transferencia o Crédito |
| **IVA incluido** | Si está marcado, los precios ya tienen IVA. Si no, se suma al total |
| **Descuento** | Si quiere hacer un descuento, escriba el monto en guaraníes |
| **Efectivo recibido** | Solo si cobra en efectivo. Escriba con cuánto paga el cliente |

3. El sistema calcula automáticamente el **cambio** (vuelto)
4. Haga clic en **Cobrar**
5. Se imprime el ticket (si tiene impresora configurada)

### 5.5 Pausar una venta

Si el cliente no termina de decidir, puede **pausar** la venta:
- Haga clic en **Pausar** (o presione F8)
- La venta queda guardada para continuarla después

### 5.6 Ventas a crédito (fiado)

1. Asigne el cliente a la venta
2. En método de pago, elija **Crédito**
3. No hace falta poner monto recibido
4. La venta se completa y el saldo del cliente **aumenta**
5. Cuando el cliente pague, vaya a **Clientes → botón $ → Registre el pago**

---

## 6. Historial de ventas

Para ver ventas anteriores:

1. Haga clic en **Historial de Ventas** en el menú
2. Use la búsqueda para filtrar por factura, cliente o producto
3. Cada fila muestra: número de factura, fecha, cliente, total, método de pago

---

## 7. Productos — Administrar inventario

### 7.1 Ver productos

Haga clic en **Productos** en el menú. Ve una tabla con todos los productos.

### 7.2 Crear producto

1. Haga clic en **Nuevo Producto**
2. Llene los campos:

| Campo | Obligatorio | Qué poner |
|-------|-------------|----------|
| **Nombre** | Sí | Ej: "Lija N°100" |
| **Código de barras** | Sí | Si no tiene, invente uno único |
| **SKU** | No | Código interno (opcional) |
| **Categoría** | Sí | Elija de la lista |
| **Proveedor** | No | Quién lo provee (opcional) |
| **Precio de venta** | Sí | Con IVA si corresponde |
| **Costo** | No | Cuánto le costó a usted |
| **Stock mínimo** | No | Para alerta de reposición |
| **Unidad** | Sí | "unidad", "kg", "metro", "litro" |

3. Haga clic en **Crear**

### 7.3 Editar producto

1. Haga clic en el ícono de lápiz ✏️
2. Modifique los campos
3. Haga clic en **Guardar**

### 7.4 Eliminar producto

1. Haga clic en el ícono de basurero 🗑️
2. Confirme la eliminación
3. Si el producto tenía stock, se pone a cero automáticamente

---

## 8. Categorías

Las categorías agrupan productos (ej: "Caños", "Ferretería", "Pinturas").

- Haga clic en **Categorías** en el menú
- **Crear**: Haga clic en **Nueva Categoría**, escriba el nombre
- **Editar**: Ícono de lápiz ✏️
- **Eliminar**: Ícono de basurero 🗑️

---

## 9. Clientes — Manejo de crédito y cobranza

### 9.1 Ver clientes

Haga clic en **Clientes** en el menú. Ve todos los clientes con su saldo y límite de crédito.

| Columna | Significado |
|---------|------------|
| **Saldo** | Cuánto debe (rojo = debe plata, verde = saldo a favor) |
| **Límite de crédito** | Hasta cuánto puede deber |

### 9.2 Crear cliente

1. Haga clic en **Nuevo Cliente**
2. Llene nombre, documento (RUC/CI), teléfono, email, dirección
3. **Límite de crédito**: ponga `0` si no quiere darle fiado, o el monto máximo
4. Haga clic en **Crear**

### 9.3 Cobrar a un cliente (reducir saldo)

Cuando un cliente paga su deuda:

1. Busque el cliente en la tabla
2. Haga clic en el botón **$** (verde)
3. Aparece una ventana con el saldo pendiente
4. Escriba el **monto que pagó**
5. Haga clic en **Registrar Pago**
6. El saldo se actualiza automáticamente

**Importante**: No puede cobrar más de lo que debe. Si el cliente debe Gs. 100.000 y usted pone Gs. 150.000, el sistema lo rechaza.

---

## 10. Proveedores

Similar a Clientes, pero para sus proveedores de mercadería.

- **Crear**: nombre, RUC, teléfono, email, dirección
- **Editar** y **Eliminar**: igual que clientes

---

## 11. Inventario — Movimientos de stock

Aquí controla las entradas y salidas de mercadería.

### 11.1 Ver movimientos

Haga clic en **Inventario** en el menú. Ve una tabla con todas las entradas, salidas y ajustes.

### 11.2 Hacer un movimiento

1. Haga clic en **Nuevo Movimiento**
2. Elija el tipo:

| Tipo | Cuándo usarlo |
|------|--------------|
| **Entrada** | Llegó mercadería nueva |
| **Salida** | Se vendió, se rompió, se devolvió |
| **Ajuste** | Corrección de stock (inventario físico) |

3. Seleccione el **producto**
4. Ponga la **cantidad** (siempre positiva, ej: `5`, `10`)
5. Ponga la **razón** (ej: "Compra al proveedor", "Merma")
6. Haga clic en **Crear**

---

## 12. Reportes

Para ver cómo va el negocio.

### 12.1 Tipos de reporte

| Reporte | Qué muestra |
|---------|------------|
| **Ventas por período** | Ventas diarias, cuánto se facturó cada día |
| **Stock bajo** | Productos que necesitan reposición urgente |
| **Margen de ganancia** | Diferencia entre costo y precio de venta |
| **Valorización de inventario** | Cuánto vale todo su stock |

### 12.2 Generar un reporte

1. Haga clic en **Reportes**
2. Elija el tipo de reporte
3. Si es de ventas o ganancia, elija fechas **Desde** y **Hasta**
4. Haga clic en **Generar**
5. Vea los resultados en pantalla

### 12.3 Exportar

- **CSV**: archivo que se abre en Excel. Cabeceras en español
- **PDF**: archivo profesional listo para imprimir o enviar

---

## 13. Usuarios del sistema

Solo el administrador puede ver esta sección.

### 13.1 Crear usuario

1. Haga clic en **Usuarios** (ícono de personas)
2. Haga clic en **Nuevo Usuario**
3. Llene:

| Campo | Descripción |
|-------|------------|
| **Usuario** | Nombre para iniciar sesión |
| **Nombre completo** | Nombre real de la persona |
| **Email** | Opcional |
| **Rol** | Admin (acceso total) o Cajero (solo ventas) |
| **Contraseña** | Mínimo 6 caracteres |

4. Haga clic en **Crear**

---

## 14. Backups — Respaldo de datos

**MUY IMPORTANTE**: Haga backups seguido para no perder información.

### 14.1 Backup automático

El sistema hace un backup **cada 4 horas** automáticamente. No tiene que hacer nada.

### 14.2 Backup manual

1. Haga clic en **Backup** en el menú
2. Haga clic en **Crear Backup**
3. Espere unos segundos
4. El backup aparece en la lista con fecha y tamaño

### 14.3 Restaurar backup

Si necesita volver a un estado anterior:

1. En la pantalla de Backup, busque el backup que quiere restaurar
2. Haga clic en el botón de restaurar (flecha hacia arriba)
3. Confirme — el sistema se reinicia con los datos restaurados

---

## 15. Auditoría — Quién hizo qué

Solo el administrador puede ver esta sección.

Esta pantalla muestra **todo** lo que pasó en el sistema:

- Quién creó/modificó/eliminó cada producto, cliente, venta
- Cuándo ocurrió (fecha y hora exacta)
- Qué cambió exactamente

Use los **filtros** para buscar por fecha, tipo de acción o entidad.

---

## 16. Configuración

En **Configuración** (ícono de engranaje) puede cambiar:

| Opción | Qué hace |
|--------|---------|
| **Datos de la empresa** | Nombre, RUC, dirección, teléfono (aparece en tickets) |
| **Tasa de IVA** | Normalmente 10% |
| **IVA incluido** | Si los precios que pone ya incluyen IVA |
| **Moneda** | Normalmente "Gs." (guaraníes) |
| **Umbral de stock bajo** | Cantidad para activar alerta de reposición |

---

## 17. Solución de problemas

### El sistema no abre

1. Verifique que la computadora esté encendida
2. Cierre el sistema y vuelva a abrirlo
3. Si sigue sin funcionar, reinicie la computadora

### "Error de conexión"

Significa que el servidor interno no está corriendo. Cierre el programa y ábralo de nuevo.

### "Sesión expirada"

Por seguridad, el sistema cierra la sesión después de un tiempo. Vuelva a iniciar sesión.

### "No tienes permiso"

Está intentando entrar a una sección que solo el administrador puede ver.

### La impresora no imprime

1. Verifique que la impresora esté encendida y conectada
2. Verifique que tenga papel
3. En el diálogo de impresión, seleccione la impresora correcta

---

## 18. Preguntas frecuentes

### ¿Necesito internet?

**No.** El sistema funciona completamente sin internet. Todo está en su computadora.

### ¿Cada cuánto debo hacer backup?

El sistema hace backups automáticos cada 4 horas. Pero si hizo muchos cambios importantes, haga un backup manual también.

### ¿Puedo usar el sistema en más de una computadora?

Actualmente el sistema está diseñado para una sola computadora. Si necesita usarlo en varias, consulte con el técnico.

### ¿Cómo cambio mi contraseña?

Haga clic en **Cambiar Contraseña** en el menú lateral (ícono de candado). Ingrese su contraseña actual y la nueva.

### ¿Qué hago si se corta la luz?

Cuando vuelva la luz, prenda la computadora y abra el sistema. Los datos no se pierden porque se guardan automáticamente en cada operación. Si tenía una venta a medio hacer, el carrito se recupera solo.

### ¿Puedo vender sin tener el producto registrado?

No. Primero debe crear el producto en **Productos → Nuevo Producto**. Después ya puede venderlo.

---

## 📞 Soporte

Si tiene dudas o problemas, contacte al equipo técnico.

---

*Sistema GVP — Punto de Venta para Ferreterías*  
*Versión 2.0 — Junio 2026*
