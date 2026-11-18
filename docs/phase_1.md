# Fase I — Pruebas de Homologación SIAT (FacturaZ)

> Fuentes:
> - Guía: https://siatinfo.impuestos.gob.bo/index.php/facturacion-en-linea/autorizacion-de-sistemas/pruebas-para-la-autorizacion-del-sistema-de-facturacion/fase-i-pruebas
> - Seguimiento (requiere sesión SIN): https://fman.impuestos.gob.bo/facturacionv2/secure/Autorizacion/SeguimientoAutorizacionSistemas.xhtml
>
> **Datos del contribuyente**
> - NIT: `7388296012`
> - Código de sistema: `228EFE45B102FC1060B2F`
> - Sistema: FacturaZ v1 (tipo PROVEEDOR) — modalidades: Computarizada en Línea + Electrónica en Línea
> - Ambiente de pruebas: `codigoAmbiente = 2` (piloto)
> - Endpoints piloto: `https://pilotosiatservicios.impuestos.gob.bo/v2/...` (V3: `/sfd/...`)
>
> **IMPORTANTE — el portal cuenta automáticamente**: cada llamada exitosa contra el piloto
> suma en el portal de seguimiento. Estado al 18/11/2026 (sin haber corrido el runner):
> - Prueba 1 (CUIS) pv=0: **1/1 — 100%** ✅ (sonda del 17/11)
> - Prueba 3 (CUFD) pv=0: **2/100 — 1%** (sondas del 17/11)
> - Verificar en el portal si el seguimiento es global o por modalidad (FacturaZ tiene dos
>   modalidades registradas; el portal muestra una sola lista → probablemente global).

## Resumen de grupos (Fase I)

| Prueba | Servicio | Casos | Repeticiones esperadas | Avance portal |
|---|---|---|---|---|
| 1. CUIS | FacturacionCodigos `solicitudCuis` | 2 | **1 por caso** | 1/2 (100%) |
| 2. Sincronización | FacturacionSincronizacion | ≥5 | **50 por caso** | 0% |
| 3. CUFD | FacturacionCodigos `solicitudCufd` | 2 | **100 por caso** | 2/100 (1%) |
| 4 … N | ? (eventos, PV, cierre, …) | ? | ? | ❌ pegar del portal |

> Fase I **no requiere firma digital (.pem)** — solo comunicación, códigos y sincronización.
> Las facturas corresponden a fases posteriores.

---

## Prueba 1 — Solicitud CUIS (2 casos, 1 correcta por caso)

Resultado esperado: **TRUE → CÓDIGO ÚNICO DE INICIO DE SISTEMAS** (codigo + fechaVigencia).
Servicio: `https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionCodigos`
Elemento SOAP: `ns:cuis > SolicitudCuis`

| N° | codigoAmbiente | codigoSucursal | codigoPuntoVenta | codigoModalidad | Estado |
|---|---|---|---|---|---|
| 1 | 2 | 0 | **1** | su modalidad | ⏳ pendiente (0/1) |
| 2 | 2 | 0 | **0** | su modalidad | ✅ 1/1 — 100% |

## Prueba 2 — Sincronización (≥5 casos, 50 correctas por caso)

Todos con: `cuis = su CUIS`, `codigoAmbiente = 2`, `codigoSistema`, `nit`, `codigoSucursal = 0`.
Servicio: `https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionSincronizacion`

| N° | Método (inferido) | codigoPuntoVenta | Resultado esperado | Estado |
|---|---|---|---|---|
| 1 | sincronizarListaActividades | 1 | LISTADO TOTAL DE ACTIVIDADES | 0/50 |
| 2 | sincronizarListaActividades | 0 | LISTADO TOTAL DE ACTIVIDADES | 0/50 |
| 3 | ? (pegar del portal) | ? | ? | ? |
| 4 | sincronizarFechaHora | 0 | FECHA Y HORA ACTUAL | 0/50 |
| 5 | sincronizarListaActividadesDocumentoSector | 1 | LISTADO ACTIVIDADES DOCUMENTO SECTOR | 0/50 |

Candidatos probables para completar (confirmar contra el portal): `sincronizarListaParametrica`,
`sincronizarListaLeyendasFactura`, `sincronizarListaMensajesServicios`,
`sincronizarListaProductosServicios`.

## Prueba 3 — Solicitud CUFD (2 casos, 100 correctas por caso)

Servicio: `FacturacionCodigos`, elemento `ns:cufd > SolicitudCufd` (requiere CUIS vigente).
Respuesta esperada: codigo + codigoControl + direccion + fechaVigencia (vigencia 24h).

| N° | codigoAmbiente | codigoSucursal | codigoPuntoVenta | codigoModalidad | Estado |
|---|---|---|---|---|---|
| 1 | 2 | 0 | **1** | su modalidad | 0/100 |
| 2 | 2 | 0 | **0** | su modalidad | 2/100 (1%) |

> Nota: 100 llamadas de CUFD generan 100 códigos. Confirmar con el SIN
> (siat.facturacion@impuestos.gob.bo) si la repetición mecánica es válida y si
> hay rate limit recomendado entre llamadas.

## Pruebas 4 … N — PENDIENTES

- [ ] Pegar del portal de seguimiento los grupos restantes (parámetros + esperado + repeticiones)
- [ ] Caso N°3 de la prueba 2 (sin detalle en el paste)
- [ ] Confirmar seguimiento global vs por modalidad

---

## Estrategia de automatización — `tools/SiatHomologacionRunner/`

App de consola en `siat-dotnet` que referencia el SDK (dogfooding: ejercita el SDK como un
cliente real).

**Config** (user-secrets o env):
```
SIAT__NIT=7388296012
SIAT__CODIGO_SISTEMA=228EFE45B102FC1060B2F
SIAT__TOKEN=<token delegado>
SIAT__AMBIENTE=Pruebas
SIAT__MODALIDAD=ComputarizadaEnLinea
```

**Comportamiento**:
1. Tabla de casos en código (prueba, caso, parámetros, repeticiones requeridas, verificación).
2. Ejecuta cada caso hasta completar sus repeticiones (1/50/100 según el grupo),
   con rate limit prudente (~300-500 ms entre llamadas).
3. Criterio de éxito por caso:
   - CUIS/CUFD: `transaccion=true` + `codigo` no vacío + `fechaVigencia` futura.
   - Sincronización: `transaccion=true` + lista no vacía (o fecha parseable en FechaHora).
   - Warnings SIAT no fallan; errores sí (se registran con código+descripcion).
4. Progreso local en `homologacion-progress.json` (reanudable) + **reporte final**
   idéntico a la tabla del portal (`prueba / caso / correctas / esperadas / %`).
5. Flags: `--only <prueba>` (ejecutar un grupo), `--loop N`, `--dry-run` (arma requests sin red).
6. **Verificación cruzada**: comparar reporte local vs portal — deben coincidir.
