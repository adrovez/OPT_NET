# Glosario de dominio — OPT

Términos de negocio del sistema, extraídos del análisis de `old/Fuente/` y `old/BD/`. Los nombres entre paréntesis son las entidades/tablas legacy correspondientes, útiles como referencia cruzada al leer el código antiguo — no son necesariamente los nombres que tendrá el esquema nuevo (ver `.agents/decisions/0003-mejoras-base-de-datos.md`).

## Organización

- **Empresa** (`OPT_Empresa`) — entidad institucional cliente de la óptica (p. ej. una empresa que deriva pacientes/beneficiarios). No es la óptica misma.
- **Sucursal** (`OPT_Sucursal`) — punto de atención físico de la óptica. Una sucursal puede ser "matriz". Los usuarios se asignan a una o más sucursales (`OPT_UsuarioSucursal`).
- **Usuario** (`OPT_Usuario`) — persona que opera el sistema (no es el cliente/paciente). Tiene un rol (`OPT_Rol`) que determina permisos.
- **Región / Comuna** (`OPT_Region`, `OPT_Comuna`) — división geográfica administrativa (Chile), usada para direcciones de clientes.

## Clientes y atención clínica

- **Cliente** (`OPT_Cliente`) — persona atendida por la óptica (paciente/beneficiario). Identificado en el legacy por RUT (ver `0003` para la corrección propuesta de esto).
- **Anamnesis** (`OPT_Anamnesis`) — ficha de antecedentes de salud del cliente relevantes para la atención óptica (hipertensión, diabetes, alergias, uso de lentes previo, observaciones).
- **Atención** (`OPT_Atencion`) — evento de atención clínica a un cliente en el legacy; podía derivar en una Anamnesis, una Receta de Cristales y/o una Orden de Trabajo. **No tiene tabla ni endpoint equivalente en el sistema nuevo** — `Anamnesis`/`RecetaCristales` se vinculan directo a `Cliente`, sin este evento intermedio. Confirmado como fuera de alcance dos veces: al migrar datos (sesión 2026-08-26 — era un bridge delgado en el legacy, solo 1.184 de 13.183 filas de `RecetaCristales` pasaban por ahí) y otra vez al diseñar las APIs de Clientes/Anamnesis/RecetaCristales (mismo día) — el frontend orquesta la creación conjunta llamando a los 3 endpoints por separado. Se conserva esta entrada solo como referencia al leer `old/Fuente/`.
- **Receta de Cristales** (`OPT_RecetaCristales`) — prescripción óptica del cliente: valores de esfera/cilindro/eje para lejos y cerca, por ojo (OD = ojo derecho, OI = ojo izquierdo), más indicadores de urgencia y de si requiere laboratorio. Pertenece siempre a un **Cliente**, y además puede estar **vinculada a una Orden de Trabajo** (`OrdenDeTrabajoId`, nullable — reproduce el `idOT` del legacy, recuperado en el script `006`): es la graduación con la que se fabricaron *esos* cristales, no simplemente la última del cliente. Una receta sin OT es la que quedó tomada en la ficha clínica y todavía no se emitió en ninguna orden; una OT puede tener más de una receta (2 casos en los datos migrados). Ver ADR `0008`.
- **Beneficiario** — persona a favor de quien se emite una Orden de Trabajo, que puede no ser el mismo Cliente titular (p. ej. un hijo beneficiario de un plan de la Empresa). Es **texto libre**, no una referencia a otro Cliente: vive en `OPT_OrdenDeTrabajo.Beneficiario` (`nvarchar(100)`, agregada en `004`).

## Núcleo comercial (Orden de Trabajo)

- **Orden de Trabajo / OT** (`OPT_OrdenDeTrabajo`) — el documento comercial central del sistema: registra qué se vendió/encargó a un cliente, su precio, abono, saldo, forma de pago y estado. Es el eje que conecta cliente, productos, pagos y receta.
- **Detalle de OT** (`OPT_OrdenDeTrabajoDetalle`; `OPT_DetalleOT` en el esquema nuevo) — línea de producto dentro de una Orden de Trabajo (cantidad, valor unitario, producto), más un **Comentario** de texto libre: la anotación del mesón sobre el armazón concreto (modelo y color, p. ej. `"FORMOSA F4 C2"`). El legacy llamaba "Lentes" a esta pestaña en su pantalla de detalle; el nombre correcto es **Detalle** (confirmado con el usuario, 2026-08-28).
- **Abono** (`OPT_Abono`) — **pago inicial** registrado contra una Orden de Trabajo al momento de crearla, con su forma de pago. Es el único de los dos conceptos que el legacy suma en `OrdenDeTrabajo.Abono`/`Saldo`.
- **Pago** (`OPT_Pago`) — **pago posterior** al abono inicial, contra la misma OT. La ambigüedad Abono vs. Pago quedó resuelta con los datos reales (sesión 2026-08-27, ADR `0006`): son dos flujos distintos y se modelan como dos tablas separadas en el esquema nuevo. Ojo: en el legacy los pagos **no** se descontaban del saldo de la OT — el sistema nuevo recalcula `TotalAbonado = abonos + pagos`.
- **Cuota** (`OPT_Cuota`) — si el pago de una OT se fracciona, cada cuota tiene número, valor, fecha de vencimiento y estado. En la práctica el legacy la usaba solo como **calendario de vencimientos**: generaba el plan al crear la OT y nunca lo actualizaba (las 34.110 filas migradas están en `PENDIENTE` con fecha de pago nula), porque los pagos reales se registraban en `OPT_Pago`. `OrdenDeTrabajo.NumeroCuotas` guarda la cantidad de cuotas pactadas.
- **Estado de Cuota** (`OPT_EstadoCuota`, solo en el esquema nuevo) — catálogo PENDIENTE / PAGADA / ANULADA. El legacy tenía este dato como texto libre; se convierte en catálogo por la regla del proyecto de no dejar estados como texto (ADR `0006`).
- **Estado de OT** (`OPT_EstadoOT`) — catálogo de estados por los que pasa una Orden de Trabajo a lo largo de su ciclo de vida.
- **Bitácora de OT** (`OPT_BitacoraOT`) — historial de cambios de estado de una Orden de Trabajo, con responsable, fecha y observación — es el registro de auditoría específico de este módulo (a diferencia del resto del sistema, que no tiene auditoría genérica).
- **Forma de Pago** (`OPT_FormaPago`) — catálogo de medios de pago aceptados. Ids 0-4 heredados del legacy (SIN INFORMACION, EFECTIVO, TARJETA CREDITO, TARJETA DEBITO, TRANSFERENCIA) + `5 = CHEQUE`, agregado en `004_comercial_pagos_cuotas.sql` porque el legacy lo usaba como texto libre en `OPT_Pago.TipoPago` sin tenerlo en su propio catálogo.
- **Ticket de OT** (`Areas/Imprimir/Rpt/rptTicketOT.rdlc` en el legacy) — comprobante que se entrega al cliente al ingresar la orden: N° de OT, cliente y teléfono, fechas de atención y entrega, detalle, receta y el resumen de dinero (total, abonado, saldo y plan de cuotas). En el sistema nuevo **no es un reporte del servidor**: es el componente Angular `app-ticket-ot`, que el navegador imprime (ADR `0009`).
- **Paquete de Cristales** (`OPT_PaqueteCristales`, `OPT_PaqueteCristalesDetalle`) — agrupación de cristales enviados a un proveedor/laboratorio externo para su elaboración, con fecha de inicio y término, asociado a una o más Órdenes de Trabajo.

## Inventario

- **Producto** (`OPT_Producto`) — ítem de catálogo (armazón, cristal, accesorio), identificado por código único, con indicador de si lleva control de stock.
- **Producto por Sucursal** (`OPT_ProductoSucursal`) — stock de un producto en una sucursal específica: entradas, salidas, stock actual, stock mínimo/máximo y precio de venta.
- **Documento de Producto** (`OPT_ProductoDocumento`) — documento de ingreso o egreso de productos (factura, guía), con proveedor y tipo de documento.
- **Ingreso/Egreso de Producto** (`OPT_ProductoIngresoEgreso`) — línea de movimiento de stock asociada a un documento, con cantidad y valores unitario/venta.
- **Regularización de Producto** (`OPT_RegularizaProducto`) — ajuste manual de stock (y opcionalmente de precio) de un producto, con responsable y motivo — es el mecanismo de corrección de inventario del legacy.
- **Nota de Traslado** (`OPT_NotaTraslado`, `OPT_NotaTrasladoDetalle`) — movimiento de productos entre dos sucursales (origen/destino), con estado de recepción.

## Integraciones

- **Defontana** (`OPT_Defontana`) — integración con el sistema contable Defontana; la tabla persiste URI, usuario y clave de acceso a esa API (hallazgo de seguridad — ver `0003`).
