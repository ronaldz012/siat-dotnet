# MiEmpresa.Siat.Sdk

SDK agnóstico (**stateless**) en .NET para integrarse con los servicios SOAP del **SIAT (Sistema de
Impuestos Nacionales — Bolivia)**. El SDK hace el trabajo pesado: arma los sobres SOAP, serializa
los documentos fiscales a XML, calcula el hash SHA-256, comprime (GZIP) y consume los servicios
web del SIN. **No persiste ningún estado**: cada método recibe todo lo que necesita como parámetro
(NIT, sistema, token, CUIS, CUFD, etc.).

> Estado actual: `0.1.0-dev` — en desarrollo. Cubre facturación **computarizada en línea**.
> Facturación electrónica (XMLDSig), paquetes de contingencia/masivos y notas de crédito-débito
> están en el roadmap (ver `Roadmap` abajo).

## Servicios soportados

| Servicio SIN | Métodos del SDK | Estado |
|---|---|---|
| `FacturacionCodigos` | `ObtenerCuisAsync`, `ObtenerCufdAsync` | ✅ |
| `ServicioFacturacionCompraVenta` (sector 1) | `EmitirFacturaComputarizadaAsync` | ✅ |
| `FacturacionSincronizacion` | `SincronizarCatalogosAsync` | ✅ |
| `FacturacionOperaciones` | `RegistrarEventoSignificativoAsync` | ✅ |
| `ServicioFacturacionDocumentoAjuste` | anulación / reversión / nota crédito-débito | ⏳ roadmap |
| Paquetes contingencia (≤500) y masivo (≤1000) | — | ⏳ roadmap |
| Electrónica en línea (XMLDSig + certificado) | — | ⏳ roadmap |

Ambientes: **Piloto (pruebas)** y **Producción**, configurables vía `SiatOptions`.
Versiones SOAP **V2** (`/v2/`) y **V3** (`/sfd/`): mismo contrato, solo cambia la ruta — el SDK
soporta ambas vía `ServiceVersion`.

## Instalación

```bash
# Aún no publicado en NuGet — se publicará como 0.x preview cuando esté completo el MVP
dotnet add package MiEmpresa.Siat.Sdk
```

## Uso rápido

```csharp
// Program.cs — registro del SDK
services.AddSiatSdk(opt =>
{
    opt.DefaultAmbiente = "Pruebas";   // o "Produccion"
    opt.TimeoutSeconds  = 30;
});

// Obtener CUIS y CUFD
var sdk = serviceProvider.GetRequiredService<ISiatSdk>();

var cuis = await sdk.ObtenerCuisAsync(new CuisRequest
{
    Nit            = 7388296012,
    CodigoSistema  = "TU-CODIGO-DE-SISTEMA",
    Ambiente       = Ambiente.Pruebas,
    Modalidad      = Modalidad.ComputarizadaEnLinea,
}, tokenDelegado, ct);

var cufd = await sdk.ObtenerCufdAsync(new CufdRequest
{
    Nit            = 7388296012,
    CodigoSistema  = "TU-CODIGO-DE-SISTEMA",
    Ambiente       = Ambiente.Pruebas,
    Modalidad      = Modalidad.ComputarizadaEnLinea,
    Cuis           = cuis.Codigo,
}, tokenDelegado, ct);

// Emitir factura computarizada (sin firma digital)
var resultado = await sdk.EmitirFacturaComputarizadaAsync(new FacturaRequest
{
    Cabecera = new FacturaCabeceraRequest
    {
        NitEmisor = 7388296012,
        RazonSocialEmisor = "MI EMPRESA S.R.L.",
        Municipio = "Oruro",
        Telefono = "74112065",
        NumeroFactura = 1,
        Cufd = cufd.Codigo,
        Cuis = cuis.Codigo,
        CodigoSistema = "TU-CODIGO-DE-SISTEMA",
        Ambiente = Ambiente.Pruebas,
        Modalidad = Modalidad.ComputarizadaEnLinea,
        CodigoSucursal = 0,
        CodigoPuntoVenta = 0,
        FechaEmision = DateTime.Now,
        NombreRazonSocialCliente = "CLIENTE",
        CodigoTipoDocumentoIdentidad = TipoDocumentoIdentidad.Nit,
        NumeroDocumentoCliente = "1022334455",
        CodigoCliente = "000001",
        CodigoMetodoPago = CodigoMetodoPago.Efectivo,
        MontoTotal = 100,
        CodigoMoneda = CodigoMoneda.Boliviano,
        TipoCambio = 1,
        MontoTotalMoneda = 100,
        MontoTotalSujetoIva = 100,
        CodigoDocumentoSector = (int)DocumentoSector.CompraVenta,
        TipoFacturaDocumento = TipoFacturaDocumento.ConDerechoCreditoFiscal,
        CodigoEmision = TipoEmision.Online,
    },
    Detalles = new[]
    {
        new FacturaDetalleRequest
        {
            ActividadEconomica = 62010,
            CodigoProductoSin  = 53161,
            CodigoUnidadMedida = 71, // Unidad
            Cantidad = 1,
            CodigoProducto = "PROD-001",
            Descripcion = "Producto de prueba",
            PrecioUnitario = 100,
            SubTotal = 100,
        },
    }
}, tokenDelegado, ct);

if (resultado.Exitoso)
    Console.WriteLine($"CUF: {resultado.Cuf} — Recepción: {resultado.CodigoRecepcion}");
```

## Manejo de errores

- **Excepciones** (`SiatException` y subtipos) para lo excepcional: `SiatCommunicationException`
  (red/HTTP), `SiatAuthenticationException`, `SiatCertificateException`, `SiatXmlException`,
  `SiatConfigurationException`.
- **Resultados tipados** para los resultados de negocio del SIAT: `Transaccion`, mensajes con
  `EsError` / `EsWarning` y la propiedad derivada `Exitoso` en cada respuesta.

## Persistencia: responsabilidad del llamador

El SDK es stateless por diseño. El CUFD (vigencia 24h), el CUIS (vigencia ~1 año) y el registro
de facturas son responsabilidad del sistema que consume el SDK. Regla de oro: si algo requiere
guardar datos o decidir cuándo renovar códigos, no vive en este paquete.

## Desarrollo

```bash
dotnet build
dotnet test          # tests con HttpMessageHandler mockeado, sin red real
```

## Roadmap

- [x] MVP computarizada en línea (CUIS, CUFD, emisión, sincronización, eventos)
- [ ] `CufBuilder` local (Módulo 11 + BCD→Hex)
- [ ] Tests de CUFD y de emisión
- [ ] DocumentoAjuste (anulación, reversión, nota crédito/débito)
- [ ] Paquetes de contingencia (≤500) y masivo (≤1000)
- [ ] Electrónica en línea: firma XMLDSig con `X509Certificate2`
- [ ] Validación opcional contra XSD
- [ ] Primer release `0.1.0-preview` en NuGet

## Referencias

- Documentación oficial del SIN: <https://siatinfo.impuestos.gob.bo>
- Soporte técnico SIAT: <siat.facturacion@impuestos.gob.bo>
- Endpoints piloto: `https://pilotosiatservicios.impuestos.gob.bo` (V2 `/v2/`, V3 `/sfd/`)
- Producción: `https://siatrest.impuestos.gob.bo` (V2 `/v2/`, V3 `/sfd/`)
