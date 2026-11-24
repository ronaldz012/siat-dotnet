namespace MiEmpresa.Siat.Sdk.Tests;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Operations;
using MiEmpresa.Siat.Sdk.Options;
using Xunit;

public class SincronizarCatalogosEspecialesOperationTests
{
    [Fact]
    public async Task Actividades_ParseaCodigoCaebTipoYDescripcion()
    {
        var (operation, _) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:sincronizarActividadesResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaListaActividades>
                    <transaccion>true</transaccion>
                    <listaActividades>
                      <codigoCaeb>620100</codigoCaeb>
                      <descripcion>ACTIVIDADES DE DISEÑO DE SISTEMAS</descripcion>
                      <tipoActividad>P</tipoActividad>
                    </listaActividades>
                    <listaActividades>
                      <codigoCaeb>6201000</codigoCaeb>
                      <descripcion>Actividades de programación informática</descripcion>
                      <tipoActividad>P</tipoActividad>
                    </listaActividades>
                  </RespuestaListaActividades>
                </ns2:sincronizarActividadesResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.ActividadesAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        Assert.Equal(2, resultado.Items.Count);
        Assert.Equal("620100", resultado.Items[0].CodigoCaeb);
        Assert.Equal("P", resultado.Items[0].TipoActividad);
        Assert.Equal("Actividades de programación informática", resultado.Items[1].Descripcion);
    }

    [Fact]
    public async Task ProductosServicios_ParseaActividadProductoYDescripcion()
    {
        var (operation, handler) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:sincronizarListaProductosServiciosResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaListaProductos>
                    <transaccion>true</transaccion>
                    <listaCodigos>
                      <codigoActividad>620100</codigoActividad>
                      <codigoProducto>83141</codigoProducto>
                      <descripcionProducto>SERVICIOS DE DESARROLLO DE TI</descripcionProducto>
                    </listaCodigos>
                  </RespuestaListaProductos>
                </ns2:sincronizarListaProductosServiciosResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.ProductosServiciosAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        var item = Assert.Single(resultado.Items);
        Assert.Equal("620100", item.CodigoActividad);
        Assert.Equal(83141, item.CodigoProducto);
        Assert.Equal("SERVICIOS DE DESARROLLO DE TI", item.DescripcionProducto);
        Assert.Contains("<ns:sincronizarListaProductosServicios>", handler.LastRequestBody);
    }

    [Fact]
    public async Task Leyendas_ParseaActividadYDescripcion()
    {
        var (operation, _) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:sincronizarListaLeyendasFacturaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaListaParametricasLeyendas>
                    <transaccion>true</transaccion>
                    <listaLeyendas>
                      <codigoActividad>6201000</codigoActividad>
                      <descripcionLeyenda>Ley N° 453: Puedes acceder a la reclamación.</descripcionLeyenda>
                    </listaLeyendas>
                  </RespuestaListaParametricasLeyendas>
                </ns2:sincronizarListaLeyendasFacturaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.LeyendasFacturaAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        var item = Assert.Single(resultado.Items);
        Assert.Equal("6201000", item.CodigoActividad);
        Assert.Contains("Ley N° 453", item.DescripcionLeyenda);
    }

    [Fact]
    public async Task ActividadesDocumentoSector_ParseaCodigoDocumentoYTipo()
    {
        var (operation, _) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:sincronizarListaActividadesDocumentoSectorResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaListaActividadesDocumentoSector>
                    <transaccion>true</transaccion>
                    <listaActividadesDocumentoSector>
                      <codigoActividad>477300</codigoActividad>
                      <codigoDocumentoSector>1</codigoDocumentoSector>
                      <tipoDocumentoSector>FCV</tipoDocumentoSector>
                    </listaActividadesDocumentoSector>
                    <listaActividadesDocumentoSector>
                      <codigoActividad>477300</codigoActividad>
                      <codigoDocumentoSector>24</codigoDocumentoSector>
                      <tipoDocumentoSector>NCD</tipoDocumentoSector>
                    </listaActividadesDocumentoSector>
                  </RespuestaListaActividadesDocumentoSector>
                </ns2:sincronizarListaActividadesDocumentoSectorResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.ActividadesDocumentoSectorAsync(
            Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        Assert.Equal(2, resultado.Items.Count);
        Assert.Equal(1, resultado.Items[0].CodigoDocumentoSector);
        Assert.Equal("FCV", resultado.Items[0].TipoDocumentoSector);
        Assert.Equal(24, resultado.Items[1].CodigoDocumentoSector);
    }

    [Fact]
    public async Task FechaHora_ParseaYNormalizaAUtc()
    {
        var operation = CrearFechaHora("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:sincronizarFechaHoraResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaFechaHora>
                    <transaccion>true</transaccion>
                    <fechaHora>2026-11-23T20:10:00.000-04:00</fechaHora>
                  </RespuestaFechaHora>
                </ns2:sincronizarFechaHoraResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Transaccion);
        Assert.Equal(DateTimeKind.Utc, resultado.FechaHora.Kind);
        Assert.Equal(new DateTime(2026, 11, 24, 0, 10, 0, DateTimeKind.Utc), resultado.FechaHora);
    }

    private static ParametricaRequest Solicitud() => new()
    {
        Nit = 7388296012,
        CodigoSistema = "TEST-SISTEMA",
        Cuis = "CUIS-TEST",
        Ambiente = Ambiente.Pruebas,
        CodigoSucursal = 0,
        CodigoPuntoVenta = 0,
    };

    private static (SincronizarCatalogosEspecialesOperation Operation, MockHttpMessageHandler Handler) Crear(
        string responseXml)
    {
        var handler = new MockHttpMessageHandler(responseXml);
        var client = Cliente(handler);
        return (new SincronizarCatalogosEspecialesOperation(client), handler);
    }

    private static ObtenerFechaHoraOperation CrearFechaHora(string responseXml)
        => new(Cliente(new MockHttpMessageHandler(responseXml)));

    private static SiatHttpClient Cliente(MockHttpMessageHandler handler)
        => new(
            new HttpClient(handler),
            Options.Create(new SiatOptions { TimeoutSeconds = 30 }),
            NullLogger<SiatHttpClient>.Instance);
}
