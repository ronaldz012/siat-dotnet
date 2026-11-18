# siat-dotnet

Repositorio del SDK .NET para el **SIAT (SIN Bolivia)**: `MiEmpresa.Siat.Sdk`.

SDK **agnóstico y stateless**: hace todo el trabajo técnico de facturación (armado de SOAP/XML,
hashing SHA-256, compresión GZIP, mapeo de errores) y consume los servicios web del SIN.
**No guarda datos ni decide flujo de negocio** — eso vive en los sistemas que lo consumen
(por ejemplo, el futuro servicio FacturaZ).

👉 Documentación completa: [`src/MiEmpresa.Siat.Sdk/README.md`](src/MiEmpresa.Siat.Sdk/README.md)

## Estructura

```
siat-dotnet/
└── src/
    ├── MiEmpresa.Siat.Sdk/         ← librería (paquete NuGet en construcción)
    └── MiEmpresa.Siat.Sdk.Tests/   ← tests xUnit (HttpMessageHandler mockeado)
```

## Desarrollo

```bash
dotnet build
dotnet test
```

## Estado

`0.1.0-dev` — MVP de facturación computarizada en línea. Roadmap en el README del SDK.
