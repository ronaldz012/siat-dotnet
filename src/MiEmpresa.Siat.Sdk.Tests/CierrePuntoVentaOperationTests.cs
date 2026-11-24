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

public class CierrePuntoVentaOperationTests
{
    [Fact]
    public async Task Cerrar_ParseaElCodigoYLaTransaccion()
    {
        var (operation, handler) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:cierrePuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaCierrePuntoVenta>
                    <codigoPuntoVenta>1</codigoPuntoVenta>
                    <transaccion>true</transaccion>
                  </RespuestaCierrePuntoVenta>
                </ns2:cierrePuntoVentaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        Assert.Equal(1, resultado.CodigoPuntoVenta);

        Assert.Contains("<ns:cierrePuntoVenta>", handler.LastRequestBody);
        Assert.Contains("<SolicitudCierrePuntoVenta>", handler.LastRequestBody);
        Assert.Contains("<codigoPuntoVenta>1</codigoPuntoVenta>", handler.LastRequestBody);
        Assert.Equal(
            "https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionOperaciones",
            handler.LastRequestUri!.ToString());
    }

    [Fact]
    public async Task Cerrar_RechazoDelSin_DevuelveMensajes()
    {
        var (operation, _) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:cierrePuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaCierrePuntoVenta>
                    <transaccion>false</transaccion>
                    <mensajesList>
                      <codigo>970</codigo>
                      <descripcion>ERROR DE VALIDACION</descripcion>
                    </mensajesList>
                  </RespuestaCierrePuntoVenta>
                </ns2:cierrePuntoVentaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.False(resultado.Exitoso);
        var mensaje = Assert.Single(resultado.Mensajes);
        Assert.Equal(970, mensaje.Codigo);
    }

    private static CierrePuntoVentaRequest Solicitud() => new()
    {
        Nit = 7388296012,
        CodigoSistema = "TEST-SISTEMA",
        Cuis = "CUIS-TEST",
        Ambiente = Ambiente.Pruebas,
        CodigoSucursal = 0,
        CodigoPuntoVenta = 1,
    };

    private static (CierrePuntoVentaOperation Operation, MockHttpMessageHandler Handler) Crear(
        string responseXml)
    {
        var handler = new MockHttpMessageHandler(responseXml);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new SiatOptions { TimeoutSeconds = 30 });
        var client = new SiatHttpClient(httpClient, options, NullLogger<SiatHttpClient>.Instance);
        return (new CierrePuntoVentaOperation(client), handler);
    }
}
